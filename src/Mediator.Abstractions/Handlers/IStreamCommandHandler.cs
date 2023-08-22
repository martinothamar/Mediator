namespace Mediator;

public interface IStreamCommandHandler<in TCommand, out TResponse> where TCommand : IStreamCommand<TResponse>
{
    IAsyncEnumerable<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}
