using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mediator;

public sealed class ForeachAwaitPublisher : INotificationPublisher
{
    public ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
    {
        if (handlers.IsSingleHandler(out var handler))
            return handler.Handle(notification, cancellationToken);

        return Publish(handlers, notification, cancellationToken);

        static async ValueTask Publish(
            NotificationHandlers<TNotification> handlers,
            TNotification notification,
            CancellationToken cancellationToken
        )
        {
            List<Exception>? exceptions = null;
            foreach (var handler in handlers)
            {
                try
                {
                    await handler.Handle(notification, cancellationToken);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(1);
                    exceptions.Add(ex);
                }
            }
            if (exceptions is not null)
                ThrowHelper.ThrowAggregateException(exceptions);
        }
    }
}

public sealed class TaskWhenAllPublisher : INotificationPublisher
{
    public ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
    {
        if (handlers.IsSingleHandler(out var singleHandler))
            return singleHandler.Handle(notification, cancellationToken);

        if (handlers.IsArray(out var handlersArray))
        {
            ValueTask[]? tasks = null;
            var count = 0;

            foreach (var handler in handlersArray)
            {
                var task = handler.Handle(notification, cancellationToken);
                if (task.IsCompletedSuccessfully)
                    continue;

                tasks ??= new ValueTask[handlersArray.Length];
                tasks[count++] = task;
            }

            if (tasks is null)
                return default;

            return AwaitTaskArray(tasks, count);
        }
        else
        {
            List<ValueTask>? tasks = null;
            foreach (var handler in handlers)
            {
                var task = handler.Handle(notification, cancellationToken);
                if (task.IsCompletedSuccessfully)
                    continue;

                tasks ??= new List<ValueTask>(1);
                tasks.Add(task);
            }

            if (tasks is null)
                return default;

            return AwaitTaskList(tasks);
        }

        static async ValueTask AwaitTaskArray(ValueTask[] tasks, int count)
        {
            List<Exception>? exceptions = null;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    await tasks[i];
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(1);
                    exceptions.Add(ex);
                }
            }

            if (exceptions is not null)
                ThrowHelper.ThrowAggregateException(exceptions);
        }

        static async ValueTask AwaitTaskList(List<ValueTask> tasks)
        {
            List<Exception>? exceptions = null;
            for (int i = 0; i < tasks.Count; i++)
            {
                try
                {
                    await tasks[i];
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(1);
                    exceptions.Add(ex);
                }
            }

            if (exceptions is not null)
                ThrowHelper.ThrowAggregateException(exceptions);
        }
    }
}

public readonly struct NotificationHandlers<TNotification>
    where TNotification : INotification
{
    private readonly IEnumerable<INotificationHandler<TNotification>> _handlers;
    private readonly bool _isArray;

    internal readonly bool IsArray([MaybeNullWhen(false)] out INotificationHandler<TNotification>[] handlers)
    {
        if (_isArray)
        {
            Debug.Assert(_handlers is INotificationHandler<TNotification>[]);
            handlers = Unsafe.As<INotificationHandler<TNotification>[]>(_handlers);
            return true;
        }

        handlers = default;
        return false;
    }

    /// <summary>
    /// Constructs a new instance of <see cref="NotificationHandlers{TNotification}"/>.
    /// Should _NOT_ be used by user code, only by the generated code in the Mediator implementation.
    /// </summary>
    /// <param name="handlers"></param>
    public NotificationHandlers(IEnumerable<INotificationHandler<TNotification>> handlers)
    {
        _handlers = handlers;
        if (handlers is INotificationHandler<TNotification>[])
            _isArray = true;
    }

    public readonly bool IsSingleHandler([MaybeNullWhen(false)] out INotificationHandler<TNotification> handler)
    {
        if (IsArray(out var handlers) && handlers.Length == 1)
        {
            handler = handlers[0];
            return true;
        }

        handler = default;
        return false;
    }

    public readonly Enumerator GetEnumerator() => new Enumerator(in this);

    public struct Enumerator
    {
        private readonly NotificationHandlers<TNotification> _handlers;
        private IEnumerator<INotificationHandler<TNotification>>? _enumerator;
        private int _index;

        internal Enumerator(in NotificationHandlers<TNotification> handlers)
        {
            _index = -1;
            _handlers = handlers;
        }

        public readonly INotificationHandler<TNotification> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (_handlers.IsArray(out var handlers))
                {
                    case true:
                        return handlers[_index];
                    case false:
                        Debug.Assert(_enumerator is not null);
                        return _enumerator!.Current;
                }
            }
        }

        public bool MoveNext()
        {
            switch (_handlers.IsArray(out var handlers))
            {
                case true:
                    if ((uint)_index + 1 < (uint)handlers.Length)
                    {
                        _index++;
                        return true;
                    }

                    return false;
                case false:
                    if (_index == -1)
                    {
                        _enumerator = _handlers._handlers.GetEnumerator();
                        _index++;
                    }
                    Debug.Assert(_enumerator is not null);
                    return _enumerator!.MoveNext();
            }
        }
    }
}

public interface INotificationPublisher
{
    ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification;
}
