using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mediator;

/// <summary>
/// Implements a notification publisher that publishes notifications to multiple handlers using a foreach loop with async/await.
/// Tries to be efficient by avoiding unnecessary allocations and async state machines,
/// the optimal case being a single handler which completes synchronously.
/// </summary>
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

/// <summary>
/// Implements a notification publisher that uses the Task.WhenAll pattern to handle multiple notification handlers.
/// Tries to be efficient by avoiding unnecessary allocations and async state machines,
/// the optimal case being a single handler or a collection of handlers that all complete synchronously.
/// </summary>
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

/// <summary>
/// Represents a collection of notification handlers for a specific notification type.
/// Contains convenience methods for implementing the <see cref="INotificationPublisher"/> in an efficient way.
/// </summary>
/// <typeparam name="TNotification">The type of notification.</typeparam>
public readonly struct NotificationHandlers<TNotification>
    where TNotification : INotification
{
    private readonly IEnumerable<INotificationHandler<TNotification>> _handlers;
    private readonly bool _isArray;

    /// <summary>
    /// Checks if the handlers are stored as an array and retrieves them if so.
    /// </summary>
    /// <param name="handlers">The array of notification handlers, if stored as an array.</param>
    /// <returns><c>true</c> if the handlers are stored as an array; otherwise, <c>false</c>.</returns>
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
    /// Do _NOT_ invoke this manually, is only supposed to be used by the source generator.
    /// </summary>
    /// <param name="handlers"></param>
    /// <param name="isArray"></param>
    public NotificationHandlers(IEnumerable<INotificationHandler<TNotification>> handlers, bool isArray)
    {
        _handlers = handlers;
        _isArray = isArray;
    }

    /// <summary>
    /// Checks wether there is exactly 1 single handler in the collection.
    /// NOTE: if the underlying collection is not an array, this will return false.
    /// </summary>
    /// <param name="handler">The single handler, if there's a exactly 1 handler present</param>
    /// <returns><c>true</c> if there is a single handler; otherwise, <c>false</c>.</returns>
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

/// <summary>
/// Represents a notification publisher that is responsible for invoking handlers for a given notification.
/// This is called by the source generated Mediator implementation when <see cref="IPublisher.Publish{TNotification}(TNotification, CancellationToken)"/> is called.
/// Built in implementations are <see cref="ForeachAwaitPublisher"/> and <see cref="TaskWhenAllPublisher"/>.
/// Configure the desired implementation in the generator options.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Receives a notification and a collection of handlers for that notification.
    /// The implementor is responsible for invoking each handler in the collection (and how to do it).
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <param name="handlers">The collection of handlers for the notification.</param>
    /// <param name="notification">The notification to be published.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification;
}
