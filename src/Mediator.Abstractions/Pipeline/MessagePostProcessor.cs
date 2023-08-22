namespace Mediator;

public abstract class MessagePostProcessor<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var response = await next(message, cancellationToken);
        await Handle(message, response, cancellationToken);
        return response;
    }

    protected abstract ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken);
}
