using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed class SomeQueryHandler : IQueryHandler<SomeQuery, SomeResponse>
{
    public ValueTask<SomeResponse> Handle(SomeQuery Query, CancellationToken cancellationToken) =>
        new ValueTask<SomeResponse>(new SomeResponse(Query.Id));
}
