using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface IQueryHandler<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        ValueTask<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
    }
}
