using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mediator;

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
    /// Checks whether there is exactly 1 single handler in the collection.
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
