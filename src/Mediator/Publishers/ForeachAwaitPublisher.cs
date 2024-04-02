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
