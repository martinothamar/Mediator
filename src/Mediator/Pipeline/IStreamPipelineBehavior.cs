namespace Mediator;

public interface IStreamPipelineBehavior<TMessage, TResponse> where TMessage : IStreamMessage
{
    IAsyncEnumerable<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        StreamHandlerDelegate<TMessage, TResponse> next
    );
}
