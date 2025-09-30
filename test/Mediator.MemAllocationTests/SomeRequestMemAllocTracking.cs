using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.MemAllocationTests;

public sealed record SomeRequestMemAllocTracking(Guid Id) : IRequest<int>;

public sealed class SomeRequestMemAllocTrackingHandler : IRequestHandler<SomeRequestMemAllocTracking, int>
{
    public ValueTask<int> Handle(SomeRequestMemAllocTracking request, CancellationToken cancellationToken) => default;
}
