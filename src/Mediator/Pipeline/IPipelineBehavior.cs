namespace Mediator;

public interface IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    );
}

public interface IPipelineBehavior<TMessage>
    where TMessage : notnull, IMessage
{
    ValueTask Handle(TMessage message, MessageHandlerDelegate<TMessage> next, CancellationToken cancellationToken);
}
