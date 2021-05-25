using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public delegate ValueTask<TResponse> MessageHandlerDelegate<TMessage, TResponse>(TMessage message, CancellationToken cancellationToken)
        where TMessage : notnull, IMessage;
}
