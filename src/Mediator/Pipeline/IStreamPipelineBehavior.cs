using System.Collections.Generic;
using System.Threading;

namespace Mediator
{
    public interface IStreamPipelineBehavior<TMessage, TResponse>
        where TMessage : IStreamMessage<TResponse>
    {
        IAsyncEnumerable<TResponse> Handle(TMessage message, CancellationToken cancellationToken, StreamHandlerDelegate<TMessage, TResponse> next);
    }
}
