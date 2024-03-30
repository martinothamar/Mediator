using System.Diagnostics.CodeAnalysis;

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
            return handler.HandlerCallback(handler.HandlerInstance, notification, cancellationToken);

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
                    await handler.HandlerCallback(handler.HandlerInstance, notification, cancellationToken);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(1);
                    exceptions.Add(ex);
                }
            }
            if (exceptions is not null)
                throw new AggregateException(exceptions);
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
            return singleHandler.HandlerCallback(singleHandler.HandlerInstance, notification, cancellationToken);

        ValueTask[]? tasks = null;
        var count = 0;

        foreach (var handler in handlers)
        {
            var task = handler.HandlerCallback(handler.HandlerInstance, notification, cancellationToken);
            if (task.IsCompletedSuccessfully)
                continue;

            tasks ??= new ValueTask[handlers.Length];
            tasks[count++] = task;
        }

        if (tasks is null)
            return default;

        return AwaitTasks(tasks, count);

        static async ValueTask AwaitTasks(ValueTask[] tasks, int count)
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
                throw new AggregateException(exceptions);
        }
    }
}

public readonly struct NotificationHandler<TNotification>(
    object handlerInstance,
    Func<object, TNotification, CancellationToken, ValueTask> handlerCallback
)
    where TNotification : INotification
{
    public readonly object HandlerInstance { get; } = handlerInstance;

    public readonly Func<object, TNotification, CancellationToken, ValueTask> HandlerCallback { get; } =
        handlerCallback;
}

public readonly struct NotificationHandlers<TNotification>
    where TNotification : INotification
{
    private readonly object _handlerInstances;
    private readonly object _handlerCallbacks;

    public readonly int Length { get; }

    public NotificationHandlers(
        object handlerInstance,
        Func<object, TNotification, CancellationToken, ValueTask> handlerCallback
    )
    {
        _handlerInstances = handlerInstance;
        _handlerCallbacks = handlerCallback;
        Length = 1;
    }

    public NotificationHandlers(
        object[] handlerInstance,
        Func<object, TNotification, CancellationToken, ValueTask>[] handlerCallback
    )
    {
        if (handlerInstance.Length != handlerCallback.Length)
            throw new InvalidOperationException("Handler instances and callbacks must have the same length.");

        _handlerInstances = handlerInstance;
        _handlerCallbacks = handlerCallback;
        Length = handlerCallback.Length;
    }

    public readonly bool IsSingleHandler([MaybeNullWhen(false)] out NotificationHandler<TNotification> handler)
    {
        if (_handlerInstances is not object[] instances)
        {
            handler = new NotificationHandler<TNotification>(
                _handlerInstances,
                (Func<object, TNotification, CancellationToken, ValueTask>)_handlerCallbacks
            );
            return true;
        }
        handler = default;
        return false;
    }

    public Enumerator GetEnumerator() => new Enumerator(in this);

    public struct Enumerator
    {
        private int _index;
        private readonly NotificationHandlers<TNotification> _handlers;

        internal Enumerator(in NotificationHandlers<TNotification> handlers)
        {
            _index = -2;
            _handlers = handlers;
        }

        public readonly NotificationHandler<TNotification> Current =>
            _index switch
            {
                -2 => throw new InvalidOperationException("Enumeration not started."),
                -1
                    => new NotificationHandler<TNotification>(
                        _handlers._handlerInstances,
                        (Func<object, TNotification, CancellationToken, ValueTask>)_handlers._handlerCallbacks
                    ),
                _
                    => new NotificationHandler<TNotification>(
                        ((object[])_handlers._handlerInstances)[_index],
                        ((Func<object, TNotification, CancellationToken, ValueTask>[])_handlers._handlerCallbacks)[
                            _index
                        ]
                    )
            };

        public bool MoveNext()
        {
            if (_index == -2)
            {
                if (_handlers.IsSingleHandler(out _))
                {
                    _index = -1;
                    return true;
                }
                else
                {
                    _index = 0;
                    return true;
                }
            }
            else if (_index == -1)
            {
                return false;
            }

            var instances = (object[])_handlers._handlerInstances;
            var next = _index + 1;
            if (next < instances.Length)
            {
                _index = next;
                return true;
            }

            return false;
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
