using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed record SomeStreamingRequest(Guid Id) : IStreamRequest<SomeResponse>;

public sealed class SomeStreamingRequestHandler : IStreamRequestHandler<SomeStreamingRequest, SomeResponse>
{
    public async IAsyncEnumerable<SomeResponse> Handle(
        SomeStreamingRequest request,
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

            yield return new SomeResponse(request.Id);
        }
    }
}
