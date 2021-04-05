using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface ISender
    {
        ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        ValueTask Send(IRequest request, CancellationToken cancellationToken = default);

        ValueTask<TResponse> Send<TResponse>(ICommand<TResponse> request, CancellationToken cancellationToken = default);

        ValueTask Send(ICommand request, CancellationToken cancellationToken = default);

        ValueTask<TResponse> Send<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default);
    }

    public interface IPublisher
    {
        ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}
