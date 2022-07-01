using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed class SomeCommandHandler : ICommandHandler<SomeCommand, SomeResponse>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();

    public ValueTask<SomeResponse> Handle(SomeCommand command, CancellationToken cancellationToken)
    {
        Ids.Add(command.Id);
        return new ValueTask<SomeResponse>(new SomeResponse(command.Id));
    }
}

public sealed class SomeCommandWithoutResponseHandler : ICommandHandler<SomeCommandWithoutResponse>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();

    public ValueTask<Unit> Handle(SomeCommandWithoutResponse command, CancellationToken cancellationToken)
    {
        Ids.Add(command.Id);
        return default;
    }
}

public sealed class SomeStructCommandHandler : ICommandHandler<SomeStructCommand>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();
    internal static readonly ConcurrentBag<long> Addresses = new();

    unsafe public ValueTask<Unit> Handle(SomeStructCommand command, CancellationToken cancellationToken)
    {
        Ids.Add(command.Id);
        var addr = *(long*)&command;
        Addresses.Add(addr);
        return default;
    }
}
