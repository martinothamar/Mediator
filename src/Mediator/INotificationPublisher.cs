namespace Mediator;

public sealed class ForeachAwaitPublisher : INotificationPublisher
{
    public async ValueTask Publish<TNotification>(
        NotificationHandlerExecutor<TNotification>[] handlerExecutors,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
    {
        List<Exception>? exceptions = null;
        foreach (var handler in handlerExecutors)
        {
            try
            {
                await handler.HandlerCallback(notification, cancellationToken);
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

public sealed class TaskWhenAllPublisher : INotificationPublisher
{
    public ValueTask Publish<TNotification>(
        NotificationHandlerExecutor<TNotification>[] handlerExecutors,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
    {
        ValueTask[]? tasks = null;
        var count = 0;

        for (int i = 0; i < handlerExecutors.Length; i++)
        {
            ref readonly var handler = ref handlerExecutors[i];
            var task = handler.HandlerCallback(notification, cancellationToken);
            if (task.IsCompletedSuccessfully)
                continue;

            tasks ??= new ValueTask[handlerExecutors.Length];
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

public readonly struct NotificationHandlerExecutor<TNotification>(
    object handlerInstance,
    Func<TNotification, CancellationToken, ValueTask> handlerCallback
)
    where TNotification : INotification
{
    public object HandlerInstance { get; } = handlerInstance;

    public Func<TNotification, CancellationToken, ValueTask> HandlerCallback { get; } = handlerCallback;
}

public interface INotificationPublisher
{
    ValueTask Publish<TNotification>(
        NotificationHandlerExecutor<TNotification>[] handlerExecutors,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification;
}
