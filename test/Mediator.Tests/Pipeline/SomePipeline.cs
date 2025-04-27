using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;

namespace Mediator.Tests.Pipeline;

public sealed class SomePipeline(bool earlyReturn = false)
    : IPipelineBehavior<SomeRequest, SomeResponse>,
        IPipelineTestData
{
    public Guid Id { get; private set; }
    public long LastMsgTimestamp { get; private set; }
    private readonly bool _earlyReturn = earlyReturn;

    public ValueTask<SomeResponse> Handle(
        SomeRequest message,
        MessageHandlerDelegate<SomeRequest, SomeResponse> next,
        CancellationToken cancellationToken
    )
    {
        LastMsgTimestamp = Stopwatch.GetTimestamp();

        if (_earlyReturn)
        {
            var response = new SomeResponse(message.Id) { ReturnedEarly = true };

            return new ValueTask<SomeResponse>(response);
        }

        if (message is null || message.Id == default)
            throw new ArgumentException("Invalid input");

        Id = message.Id;

        return next(message, cancellationToken);
    }
}
