using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface ICommandHandler<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        ValueTask<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
        where TCommand : ICommand<Unit>
    {
    }

    public interface IStreamCommandHandler<in TCommand, out TResponse>
        where TCommand : IStreamCommand<TResponse>
    {
        IAsyncEnumerable<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
    }
}
