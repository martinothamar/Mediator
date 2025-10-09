using System.Runtime.CompilerServices;

namespace Mediator;

public abstract class StreamMessagePostProcessor<TMessage, TResponse> : IStreamPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IStreamMessage
{
    public async IAsyncEnumerable<TResponse> Handle(
        TMessage message,
        StreamHandlerDelegate<TMessage, TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var response in next(message, cancellationToken).WithCancellation(cancellationToken))
        {
            await Handle(message, response, cancellationToken);
            yield return response;
        }
    }

    protected abstract ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken);
}
