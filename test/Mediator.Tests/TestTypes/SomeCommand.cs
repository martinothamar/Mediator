using System;

namespace Mediator.Tests.TestTypes
{
    public sealed record SomeCommand(Guid Id) : ICommand<SomeResponse>;
    public sealed record SomeCommandWithoutResponse(Guid Id) : ICommand;
}
