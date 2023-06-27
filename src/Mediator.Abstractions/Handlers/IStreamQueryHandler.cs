namespace Mediator;

public interface IStreamQueryHandler<in TQuery, out TResponse> where TQuery : IStreamQuery<TResponse>
{
    IAsyncEnumerable<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
