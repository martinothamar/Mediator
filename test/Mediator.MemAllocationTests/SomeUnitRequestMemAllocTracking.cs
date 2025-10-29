using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.MemAllocationTests;

public sealed record SomeUnitRequestMemAllocTracking(Guid Id) : IRequest;

public sealed class SomeUnitRequestMemAllocTrackingHandler : IRequestHandler<SomeUnitRequestMemAllocTracking>
{
    public ValueTask Handle(SomeUnitRequestMemAllocTracking request, CancellationToken cancellationToken) => default;
}
