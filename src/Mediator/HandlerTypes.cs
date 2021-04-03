using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IRequestHandler<in TRequest>
        where TRequest : IRequest
    {
        ValueTask Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        ValueTask Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        ValueTask<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface IQueryHandler<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        ValueTask<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
    }

    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        ValueTask Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
