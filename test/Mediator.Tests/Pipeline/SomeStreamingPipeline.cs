using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Mediator.Tests.TestTypes;

namespace Mediator.Tests.Pipeline;

public sealed class SomeStreamingPipeline : IStreamPipelineBehavior<SomeStreamingQuery, SomeResponse>
{
    public async IAsyncEnumerable<SomeResponse> Handle(
        SomeStreamingQuery message,
        StreamHandlerDelegate<SomeStreamingQuery, SomeResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var response in next(message, cancellationToken))
        {
            response.SomeStreamingData = 1;
            yield return response;
        }
    }
}
