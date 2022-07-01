using System;

namespace Mediator.Tests.Pipeline;

public interface IPipelineTestData
{
    Guid Id { get; }

    public long LastMsgTimestamp { get; }
}
