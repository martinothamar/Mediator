namespace Mediator;

public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TMessage, TResponse>(
    TMessage message,
    CancellationToken cancellationToken
) where TMessage : notnull, IStreamMessage;
