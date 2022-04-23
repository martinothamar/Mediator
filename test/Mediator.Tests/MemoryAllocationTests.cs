using Mediator.Tests.Common;
using Mediator.Tests.TestTypes;

namespace Mediator.Tests;

[Collection("Non-Parallel")]
public class MemoryAllocationTests
{
    [Fact]
    public void Test_Request_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();
        var request = new SomeRequestMemAllocTracking(id);

        // Ensure everything is cached
        mediator.Send(request); // Everything returns sync

        var beforeBytes = Allocations.GetCurrentThreadAllocatedBytes();
        mediator.Send(request); // Everything returns sync
        var afterBytes = Allocations.GetCurrentThreadAllocatedBytes();

        Assert.Equal(0, afterBytes - beforeBytes);
    }

    //[Fact]
    //public void Test_Notification_Handler()
    //{
    //    var (_, mediator) = Fixture.GetMediator();

    //    var id = Guid.NewGuid();
    //    var notification = new SomeNotificationMemAllocTracking(id);

    //    // Ensure everything is cached
    //    mediator.Publish(notification); // Everything returns sync

    //    var beforeBytes = Allocations.GetCurrentThreadAllocatedBytes();
    //    mediator.Publish(notification); // Everything returns sync
    //    var afterBytes = Allocations.GetCurrentThreadAllocatedBytes();

    //    Assert.Equal(0, afterBytes - beforeBytes);
    //}
}
