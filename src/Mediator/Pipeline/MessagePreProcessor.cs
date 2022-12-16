namespace Mediator;

public abstract class MessagePreProcessor<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    public ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next
    )
    {
        var task = Handle(message, cancellationToken);
        if (task.IsCompletedSuccessfully)
            return next(message, cancellationToken);

        return HandleInternal(task, message, cancellationToken, next);

        static async ValueTask<TResponse> HandleInternal(
            ValueTask task,
            TMessage message,
            CancellationToken cancellationToken,
            MessageHandlerDelegate<TMessage, TResponse> next
        )
        {
            await task;
            return await next(message, cancellationToken);
        }
    }

    protected abstract ValueTask Handle(TMessage message, CancellationToken cancellationToken);
}
