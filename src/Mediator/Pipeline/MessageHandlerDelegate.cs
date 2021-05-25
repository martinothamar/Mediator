using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public delegate ValueTask<TResponse> MessageHandlerDelegate<in TMessage, TResponse>(TMessage message, CancellationToken cancellationToken)
        where TMessage : notnull, IMessage;
}
