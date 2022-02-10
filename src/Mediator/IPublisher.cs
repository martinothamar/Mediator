namespace Mediator;

public interface IPublisher
{
    ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    ValueTask Publish(object notification, CancellationToken cancellationToken = default);
}
