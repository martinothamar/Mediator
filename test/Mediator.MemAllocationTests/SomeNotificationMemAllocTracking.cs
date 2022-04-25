namespace Mediator.MemAllocationTests;

public sealed record SomeNotificationMemAllocTracking(Guid Id) : INotification;

public sealed class SomeNotificationMemAllocTrackingHandler : INotificationHandler<SomeNotificationMemAllocTracking>
{
    public ValueTask Handle(SomeNotificationMemAllocTracking notification, CancellationToken cancellationToken) =>
        default;
}
