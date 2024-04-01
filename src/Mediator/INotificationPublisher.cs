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
            return handler.Method(handler.Instance, notification, cancellationToken);

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
                    await handler.Method(handler.Instance, notification, cancellationToken);
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
            return singleHandler.Method(singleHandler.Instance, notification, cancellationToken);

        ValueTask[]? tasks = null;
        var count = 0;

        foreach (var handler in handlers)
        {
            var task = handler.Method(handler.Instance, notification, cancellationToken);
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
                ThrowHelper.ThrowAggregateException(exceptions);
        }
    }
}

public readonly struct NotificationHandler<TNotification>(
    object handlerInstance,
    Func<object, TNotification, CancellationToken, ValueTask> handlerMethod
)
    where TNotification : INotification
{
    public readonly object Instance { get; } = handlerInstance;

    public readonly Func<object, TNotification, CancellationToken, ValueTask> Method { get; } = handlerMethod;
}

public readonly struct NotificationHandlers<TNotification>
    where TNotification : INotification
{
    private readonly object _handlerInstances;
    private readonly object _handlerMethods;

    public readonly int Length => _handlerInstances is object[] instances ? instances.Length : 1;

    /// <summary>
    /// Constructs a new instance of <see cref="NotificationHandlers{TNotification}"/>.
    /// Should _NOT_ be used by user code, only by the generated code in the Mediator implementation.
    /// </summary>
    /// <param name="handlerInstance"></param>
    /// <param name="handlerMethod"></param>
    public NotificationHandlers(
        object handlerInstance,
        Func<object, TNotification, CancellationToken, ValueTask> handlerMethod
    )
    {
        _handlerInstances = handlerInstance;
        _handlerMethods = handlerMethod;
    }

    /// <summary>
    /// Constructs a new instance of <see cref="NotificationHandlers{TNotification}"/>.
    /// Should _NOT_ be used by user code, only by the generated code in the Mediator implementation.
    /// </summary>
    /// <param name="handlerInstances"></param>
    /// <param name="handlerMethods"></param>
    public NotificationHandlers(
        object[] handlerInstances,
        Func<object, TNotification, CancellationToken, ValueTask>[] handlerMethods
    )
    {
        if (handlerInstances.Length != handlerMethods.Length)
            ThrowHelper.ThrowInvalidOperationException("Handler instances and methods must have the same length.");

        _handlerInstances = handlerInstances;
        _handlerMethods = handlerMethods;
    }

    public readonly bool IsSingleHandler([MaybeNullWhen(false)] out NotificationHandler<TNotification> handler)
    {
        if (_handlerInstances is not object[] instances)
        {
            Debug.Assert(_handlerMethods is Func<object, TNotification, CancellationToken, ValueTask>);
            handler = new NotificationHandler<TNotification>(
                _handlerInstances,
                Unsafe.As<Func<object, TNotification, CancellationToken, ValueTask>>(_handlerMethods)
            );
            return true;
        }

        Debug.Assert(_handlerMethods is Func<object, TNotification, CancellationToken, ValueTask>[]);
        handler = default;
        return false;
    }

    public readonly Enumerator GetEnumerator() => new Enumerator(in this);

    public struct Enumerator
    {
        private int _index;
        private readonly NotificationHandlers<TNotification> _handlers;

        internal Enumerator(in NotificationHandlers<TNotification> handlers)
        {
            _index = -2;
            _handlers = handlers;
        }

        public readonly NotificationHandler<TNotification> Current
        {
            get
            {
                switch (_index)
                {
                    case -2:
                        ThrowHelper.ThrowInvalidOperationException("Enumeration not started.");
                        return default;
                    case -1:
                        Debug.Assert(
                            _handlers._handlerMethods is Func<object, TNotification, CancellationToken, ValueTask>
                        );
                        return new NotificationHandler<TNotification>(
                            _handlers._handlerInstances,
                            Unsafe.As<Func<object, TNotification, CancellationToken, ValueTask>>(
                                _handlers._handlerMethods
                            )
                        );
                    default:
                        Debug.Assert(_index >= 0);
                        Debug.Assert(_handlers._handlerInstances is object[]);
                        Debug.Assert(
                            _handlers._handlerMethods is Func<object, TNotification, CancellationToken, ValueTask>[]
                        );
                        return new NotificationHandler<TNotification>(
                            Unsafe.As<object[]>(_handlers._handlerInstances)[_index],
                            Unsafe.As<Func<object, TNotification, CancellationToken, ValueTask>[]>(
                                _handlers._handlerMethods
                            )[_index]
                        );
                }
            }
        }

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

            Debug.Assert(_index >= 0);
            Debug.Assert(_handlers._handlerInstances is object[]);
            var instances = Unsafe.As<object[]>(_handlers._handlerInstances);
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
