using Mediator.Tests.TestTypes;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mediator.Tests.Pipeline;

public sealed class SomeStreamingPipeline : IStreamPipelineBehavior<SomeStreamingQuery, SomeResponse>
{
    public async IAsyncEnumerable<SomeResponse> Handle(
        SomeStreamingQuery message,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        StreamHandlerDelegate<SomeStreamingQuery, SomeResponse> next
    )
    {
        await foreach (var response in next(message, cancellationToken))
        {
            response.SomeStreamingData = 1;
            yield return response;
        }
    }
}
