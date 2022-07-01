using System;

namespace Mediator.Tests.TestTypes;

public sealed record SomeResponse(Guid Id)
{
    public int SomeStreamingData { get; set; }
}
