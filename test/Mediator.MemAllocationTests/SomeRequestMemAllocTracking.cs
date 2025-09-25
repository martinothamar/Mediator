using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.MemAllocationTests;

public sealed record SomeRequestMemAllocTracking(Guid Id) : IRequest;

public sealed class SomeRequestMemAllocTrackingHandler : IRequestHandler<SomeRequestMemAllocTracking>
{
    public ValueTask Handle(SomeRequestMemAllocTracking request, CancellationToken cancellationToken) => default;
}
