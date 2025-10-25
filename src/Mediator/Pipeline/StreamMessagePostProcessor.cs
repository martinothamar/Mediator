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
        var responses = new List<TResponse>();
        await foreach (var response in next(message, cancellationToken).WithCancellation(cancellationToken))
        {
            responses.Add(response);
            yield return response;
        }

        await Handle(message, responses, cancellationToken);
    }

    protected abstract ValueTask Handle(
        TMessage message,
        IReadOnlyList<TResponse> responses,
        CancellationToken cancellationToken
    );
}
