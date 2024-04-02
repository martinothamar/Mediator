namespace Mediator;

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
            if (handlersArray.Length == 2)
            {
                var task0 = handlersArray[0].Handle(notification, cancellationToken);
                var task1 = handlersArray[1].Handle(notification, cancellationToken);
                if (task0.IsCompletedSuccessfully && task1.IsCompletedSuccessfully)
                    return default;
                return AwaitTasks(task0, task1);
            }
            if (handlersArray.Length == 3)
            {
                var task0 = handlersArray[0].Handle(notification, cancellationToken);
                var task1 = handlersArray[1].Handle(notification, cancellationToken);
                var task2 = handlersArray[2].Handle(notification, cancellationToken);
                if (task0.IsCompletedSuccessfully && task1.IsCompletedSuccessfully && task2.IsCompletedSuccessfully)
                    return default;
                return AwaitTasks(task0, task1, task2);
            }
            if (handlersArray.Length == 4)
            {
                var task0 = handlersArray[0].Handle(notification, cancellationToken);
                var task1 = handlersArray[1].Handle(notification, cancellationToken);
                var task2 = handlersArray[2].Handle(notification, cancellationToken);
                var task3 = handlersArray[3].Handle(notification, cancellationToken);
                if (
                    task0.IsCompletedSuccessfully
                    && task1.IsCompletedSuccessfully
                    && task2.IsCompletedSuccessfully
                    && task3.IsCompletedSuccessfully
                )
                    return default;
                return AwaitTasks(task0, task1, task2, task3);
            }

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
    }

    static async ValueTask AwaitTasks(ValueTask task0, ValueTask task1)
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
        try
        {
            await task1;
        }
        catch (Exception ex)
        {
            exceptions ??= new List<Exception>(1);
            exceptions.Add(ex);
        }

        if (exceptions is not null)
            ThrowHelper.ThrowAggregateException(exceptions);
    }

    static async ValueTask AwaitTasks(ValueTask task0, ValueTask task1, ValueTask task2)
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
        try
        {
            await task1;
        }
        catch (Exception ex)
        {
            exceptions ??= new List<Exception>(1);
            exceptions.Add(ex);
        }
        try
        {
            await task2;
        }
        catch (Exception ex)
        {
            exceptions ??= new List<Exception>(1);
            exceptions.Add(ex);
        }

        if (exceptions is not null)
            ThrowHelper.ThrowAggregateException(exceptions);
    }

    static async ValueTask AwaitTasks(ValueTask task0, ValueTask task1, ValueTask task2, ValueTask task3)
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
        try
        {
            await task1;
        }
        catch (Exception ex)
        {
            exceptions ??= new List<Exception>(1);
            exceptions.Add(ex);
        }
        try
        {
            await task2;
        }
        catch (Exception ex)
        {
            exceptions ??= new List<Exception>(1);
            exceptions.Add(ex);
        }
        try
        {
            await task3;
        }
        catch (Exception ex)
        {
            exceptions ??= new List<Exception>(1);
            exceptions.Add(ex);
        }

        if (exceptions is not null)
            ThrowHelper.ThrowAggregateException(exceptions);
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
