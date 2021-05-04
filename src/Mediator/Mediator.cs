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

    //public abstract class MediatorBase : ISender, IPublisher
    //{
    //    private readonly IServiceProvider _sp;

    //    public MediatorBase(IServiceProvider sp)
    //    {
    //        _sp = sp;
    //    }

    //    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public ValueTask Send(IRequest request, CancellationToken cancellationToken = default)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public ValueTask<TResponse> Send<TResponse>(ICommand<TResponse> request, CancellationToken cancellationToken = default)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public ValueTask Send(ICommand request, CancellationToken cancellationToken = default)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public ValueTask<TResponse> Send<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
}
