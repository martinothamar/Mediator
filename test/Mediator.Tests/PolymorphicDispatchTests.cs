using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public sealed class PolymorphicDispatchTests
{
    public interface IPolymorphicDispatchNotification : INotification
    {
        Guid Id { get; }
    }

    public sealed record SomeNotification(Guid Id) : IPolymorphicDispatchNotification;

    public sealed record SomeOtherNotification(Guid Id) : IPolymorphicDispatchNotification;

    public readonly record struct SomeThirdNotification(Guid Id) : IPolymorphicDispatchNotification;

    public sealed class SomePolymorphicNotificationHandler : INotificationHandler<IPolymorphicDispatchNotification>
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();
        internal readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public ValueTask Handle(IPolymorphicDispatchNotification notification, CancellationToken cancellationToken)
        {
            Ids.Add(notification.Id);
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }
    }

    [Fact]
    public async Task Test_Multiple_Notification_Handlers()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var notification1 = new SomeNotification(Guid.NewGuid());
        var notification2 = new SomeOtherNotification(Guid.NewGuid());
        var notification3 = new SomeThirdNotification(Guid.NewGuid());

        var handler = sp.GetRequiredService<SomePolymorphicNotificationHandler>();
        Assert.NotNull(handler);

        await mediator.Publish(notification1, TestContext.Current.CancellationToken);
        Assert.Contains(notification1.Id, SomePolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler.InstanceIds, notification1.Id);

        await mediator.Publish(notification2, TestContext.Current.CancellationToken);
        Assert.Contains(notification2.Id, SomePolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler.InstanceIds, notification2.Id);

        // Polymorphic dispatch does not work with structs because
        // `SomePolymorphicNotificationHandler` is not convertible to `INotificationHandler<SomeThirdNotification>`, but
        // `SomePolymorphicNotificationHandler` is in fact convertible to `INotificationHandler<SomeOtherNotification>`
        await mediator.Publish(notification3, TestContext.Current.CancellationToken);
        Assert.DoesNotContain(notification3.Id, SomePolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(0, handler.InstanceIds, notification3.Id);

        await mediator.Publish(notification1, TestContext.Current.CancellationToken);
        AssertInstanceIdCount(2, handler.InstanceIds, notification1.Id);
    }

    [Fact]
    public async Task Test_Multiple_Notification_Handlers_With_Object_Notification()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var notification1 = new SomeNotification(Guid.NewGuid());
        var notification2 = new SomeOtherNotification(Guid.NewGuid());
        var notification3 = new SomeThirdNotification(Guid.NewGuid());

        var handler = sp.GetRequiredService<SomePolymorphicNotificationHandler>();
        Assert.NotNull(handler);

        await mediator.Publish((object)notification1, ct);
        Assert.Contains(notification1.Id, SomePolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler.InstanceIds, notification1.Id);

        await mediator.Publish((object)notification2, ct);
        Assert.Contains(notification2.Id, SomePolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler.InstanceIds, notification2.Id);

        await mediator.Publish(notification3, ct);
        Assert.DoesNotContain(notification3.Id, SomePolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(0, handler.InstanceIds, notification3.Id);

        await mediator.Publish(notification1, ct);
        AssertInstanceIdCount(2, handler.InstanceIds, notification1.Id);
    }
}
