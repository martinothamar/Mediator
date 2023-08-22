namespace Mediator;

public abstract class MessagePreProcessor<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    public ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var task = Handle(message, cancellationToken);
        if (task.IsCompletedSuccessfully)
            return next(message, cancellationToken);

        return HandleInternal(task, message, next, cancellationToken);

        static async ValueTask<TResponse> HandleInternal(
            ValueTask task,
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken cancellationToken
        )
        {
            await task;
            return await next(message, cancellationToken);
        }
    }

    protected abstract ValueTask Handle(TMessage message, CancellationToken cancellationToken);
}
