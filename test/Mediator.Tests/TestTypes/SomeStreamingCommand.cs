using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed record SomeStreamingCommand(Guid Id) : IStreamCommand<SomeResponse>;

public sealed class SomeStreamingCommandHandler : IStreamCommandHandler<SomeStreamingCommand, SomeResponse>
{
    public async IAsyncEnumerable<SomeResponse> Handle(
        SomeStreamingCommand command,
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
