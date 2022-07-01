using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed record SomeStreamingQuery(Guid Id) : IStreamQuery<SomeResponse>;

public sealed class SomeStreamingQueryHandler : IStreamQueryHandler<SomeStreamingQuery, SomeResponse>
{
    public async IAsyncEnumerable<SomeResponse> Handle(
        SomeStreamingQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await Task.Delay(100, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                yield break;
            }

            yield return new SomeResponse(query.Id);
        }
    }
}
