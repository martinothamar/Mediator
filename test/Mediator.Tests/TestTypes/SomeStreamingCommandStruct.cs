using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public record struct SomeStreamingCommandStruct(Guid Id) : IStreamCommand<SomeResponse>;

public sealed class SomeStreamingCommandStructHandler : IStreamCommandHandler<SomeStreamingCommandStruct, SomeResponse>
{
    public async IAsyncEnumerable<SomeResponse> Handle(
        SomeStreamingCommandStruct command,
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

            yield return new SomeResponse(command.Id);
        }
    }
}
