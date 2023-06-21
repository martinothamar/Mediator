namespace Mediator;

public interface IStreamPipelineBehavior<TMessage, TResponse> where TMessage : IStreamMessage
{
    IAsyncEnumerable<TResponse> Handle(
        TMessage message,
        StreamHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    );
}
