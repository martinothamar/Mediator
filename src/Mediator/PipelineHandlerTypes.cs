using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public delegate ValueTask<TResponse> MessageHandlerDelegate<TRequest, TResponse>(TRequest message, CancellationToken cancellationToken)
        where TRequest : IMessage;

    public delegate ValueTask MessageHandlerDelegate<TRequest>(TRequest message, CancellationToken cancellationToken)
        where TRequest : IMessage;

    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : IMessage
    {
        ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, TResponse> next);
    }

    public interface IPipelineBehavior<TRequest>
        where TRequest : IMessage
    {
        ValueTask Handle(TRequest request, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest> next);
    }
}
