using Mediator.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mediator.MemAllocationTests;

[Collection("Non-Parallel")]
public class NotificationTests
{
    [Fact]
    public void Test_Notification_Handler()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        using var sp = services.BuildServiceProvider(validateScopes: true);
        var mediator = sp.GetRequiredService<IMediator>();

        var id = Guid.NewGuid();
        var notification = new SomeNotificationMemAllocTracking(id);

        // Ensure everything is cached
        mediator.Publish(notification); // Everything returns sync

        var beforeBytes = Allocations.GetCurrentThreadAllocatedBytes();
        mediator.Publish(notification); // Everything returns sync
        var afterBytes = Allocations.GetCurrentThreadAllocatedBytes();

        // TODO: why 40? benchmarkdotnet reports 0 alloc for this case
        Assert.Equal(40, afterBytes - beforeBytes);
    }
}
