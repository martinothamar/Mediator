using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        var handler = sp.GetRequiredService<SomePolymorphicNotificationHandler>();
        Assert.NotNull(handler);

        await mediator.Publish(notification1);
        Assert.Contains(notification1.Id, SomePolymorphicNotificationHandler.Ids);
        if (Mediator.ServiceLifetime != ServiceLifetime.Transient)
            Assert.Equal(1, handler.InstanceIds.GetValueOrDefault(notification1.Id, 0));

        await mediator.Publish(notification2);
        Assert.Contains(notification2.Id, SomePolymorphicNotificationHandler.Ids);
        if (Mediator.ServiceLifetime != ServiceLifetime.Transient)
            Assert.Equal(1, handler.InstanceIds.GetValueOrDefault(notification2.Id, 0));

        await mediator.Publish(notification1);
        if (Mediator.ServiceLifetime != ServiceLifetime.Transient)
            Assert.Equal(2, handler.InstanceIds.GetValueOrDefault(notification1.Id, 0));
    }

    [Fact]
    public async Task Test_Multiple_Notification_Handlers_With_Object_Notification()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var notification1 = new SomeNotification(Guid.NewGuid());
        var notification2 = new SomeOtherNotification(Guid.NewGuid());

        var handler = sp.GetRequiredService<SomePolymorphicNotificationHandler>();
        Assert.NotNull(handler);

        await mediator.Publish((object)notification1);
        Assert.Contains(notification1.Id, SomePolymorphicNotificationHandler.Ids);
        if (Mediator.ServiceLifetime != ServiceLifetime.Transient)
            Assert.Equal(1, handler.InstanceIds.GetValueOrDefault(notification1.Id, 0));

        await mediator.Publish((object)notification2);
        Assert.Contains(notification2.Id, SomePolymorphicNotificationHandler.Ids);
        if (Mediator.ServiceLifetime != ServiceLifetime.Transient)
            Assert.Equal(1, handler.InstanceIds.GetValueOrDefault(notification2.Id, 0));

        await mediator.Publish(notification1);
        if (Mediator.ServiceLifetime != ServiceLifetime.Transient)
            Assert.Equal(2, handler.InstanceIds.GetValueOrDefault(notification1.Id, 0));
    }
}
