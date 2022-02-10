using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface IQueryHandler<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        ValueTask<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
    }

    public interface IStreamQueryHandler<in TQuery, out TResponse>
        where TQuery : IStreamQuery<TResponse>
    {
        IAsyncEnumerable<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
    }
}
