using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.MemAllocationTests;

[Collection("Non-Parallel")]
public class NotificationTests
{
    [Fact]
    public void Test_Notification_Handler()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        using var sp = services.BuildServiceProvider(
            new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
        );
        var mediator = sp.GetRequiredService<IMediator>();

        var id = Guid.NewGuid();
        var notification = new SomeNotificationMemAllocTracking(id);
        var cancellationToken = TestContext.Current.CancellationToken;

        // We suppress this here as we know the tasks complete sync
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        // Ensure everything is cached
        mediator.Publish(notification, cancellationToken).GetAwaiter().GetResult();

        var beforeBytes = Allocations.GetCurrentThreadAllocatedBytes();
        mediator.Publish(notification, cancellationToken).GetAwaiter().GetResult();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
        var afterBytes = Allocations.GetCurrentThreadAllocatedBytes();

        Assert.Equal(0, afterBytes - beforeBytes);
    }
}
