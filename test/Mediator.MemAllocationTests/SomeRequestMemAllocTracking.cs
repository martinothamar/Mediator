namespace Mediator.MemAllocationTests;

public sealed record SomeRequestMemAllocTracking(Guid Id) : IRequest;

public sealed class SomeRequestMemAllocTrackingHandler : IRequestHandler<SomeRequestMemAllocTracking>
{
    public ValueTask<Unit> Handle(SomeRequestMemAllocTracking request, CancellationToken cancellationToken) => default;
}
