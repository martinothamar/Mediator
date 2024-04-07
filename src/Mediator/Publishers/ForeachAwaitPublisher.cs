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

        if (handlers.IsArray(out var handlerArray))
        {
            var task0 = handlerArray[0].Handle(notification, cancellationToken);
            if (task0.IsCompletedSuccessfully)
            {
                var task1 = handlerArray[1].Handle(notification, cancellationToken);
                if (task1.IsCompletedSuccessfully)
                {
                    if (handlerArray.Length == 2)
                        return default;

                    var task2 = handlerArray[2].Handle(notification, cancellationToken);
                    if (task2.IsCompletedSuccessfully)
                    {
                        if (handlerArray.Length == 3)
                            return default;

                        var task3 = handlerArray[3].Handle(notification, cancellationToken);
                        if (task3.IsCompletedSuccessfully)
                        {
                            if (handlerArray.Length == 4)
                                return default;

                            var task4 = handlerArray[4].Handle(notification, cancellationToken);
                            return PublishArray(
                                task4.AsTask(),
                                start: 5,
                                handlerArray,
                                notification,
                                cancellationToken
                            );
                        }
                        else
                        {
                            return PublishArray(
                                task3.AsTask(),
                                start: 4,
                                handlerArray,
                                notification,
                                cancellationToken
                            );
                        }
                    }
                    else
                    {
                        return PublishArray(task2.AsTask(), start: 3, handlerArray, notification, cancellationToken);
                    }
                }
                else
                {
                    return PublishArray(task1.AsTask(), start: 2, handlerArray, notification, cancellationToken);
                }
            }

            return PublishArray(task0.AsTask(), start: 1, handlerArray, notification, cancellationToken);
        }
        else
        {
            return PublishNonArray(handlers, notification, cancellationToken);
        }
    }

    static async ValueTask PublishArray<TNotification>(
        Task task0,
        int start,
        INotificationHandler<TNotification>[] handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
    {
        List<Exception>? exceptions = null;

        try
        {
            await task0;
        }
        catch (Exception ex)
        {
            exceptions ??= new List<Exception>(1);
            exceptions.Add(ex);
        }

        for (int i = start; i < handlers.Length; i++)
        {
            try
            {
                await handlers[i].Handle(notification, cancellationToken);
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

    static async ValueTask PublishNonArray<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
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
