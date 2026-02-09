namespace Mediator;

public delegate ValueTask<TResponse> MessageHandlerDelegate<TMessage, TResponse>(
    TMessage message,
    CancellationToken cancellationToken
)
    where TMessage : notnull, IMessage;

public delegate ValueTask MessageHandlerDelegate<TMessage>(TMessage message, CancellationToken cancellationToken)
    where TMessage : notnull, IMessage;
