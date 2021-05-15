using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface ISender
    {
        ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        ValueTask Send(IRequest request, CancellationToken cancellationToken = default);

        ValueTask<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

        ValueTask Send(ICommand command, CancellationToken cancellationToken = default);

        ValueTask<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
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
    //        var sp = _sp;

    //        var handlers = sp.GetServices<INotificationHandler<TNotification>>();

    //        var count = handlers.Count();
    //        if (handlers == null || count == 0)
    //            return default;
    //        else if (count == 1)
    //            return handlers.First().Handle(notification, cancellationToken);

    //        return Publish(notification, handlers, cancellationToken);

    //        async ValueTask Publish(TNotification notification, IEnumerable<INotificationHandler<TNotification>> handlers, CancellationToken cancellationToken)
    //        {
    //            // We don't allocate the list if no task throws
    //            List<Exception>? exceptions = null;

    //            foreach (var handler in handlers)
    //            {
    //                try
    //                {
    //                    await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
    //                }
    //                catch (Exception ex)
    //                {
    //                    exceptions ??= new List<Exception>();
    //                    exceptions.Add(ex);
    //                }
    //            }

    //            if (exceptions != null)
    //                throw new AggregateException(exceptions);
    //        }
    //    }

    //    protected abstract ValueTask<TResponse> SendInternal<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    //    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
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
