using Mediator;

namespace AspNetCoreIndirect.BaseClasses;

public abstract class ApplicationHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        // For the sake of simplicity we'll just return downstream implementation
        return ProcessRequest(request, cancellationToken);
    }

    protected abstract ValueTask<TResponse> ProcessRequest(TRequest request, CancellationToken cancellationToken);
}
