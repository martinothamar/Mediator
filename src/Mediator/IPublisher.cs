namespace Mediator;

/// <summary>
/// Mediator instance for publishing notifications
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publish notification.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="AggregateException"/> if handlers throw exceptions.
    /// </summary>
    /// <param name="notification">Incoming notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task</returns>
    ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Publish notification.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="AggregateException"/> if handlers throw exception(s).
    /// Drops messages
    /// </summary>
    /// <param name="notification">Incoming notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task</returns>
    ValueTask Publish(object notification, CancellationToken cancellationToken = default);
}
