using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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

        public ValueTask Handle(IPolymorphicDispatchNotification notification, CancellationToken cancellationToken)
        {
            Ids.Add(notification.Id);
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

        await mediator.Publish(notification2);
        Assert.Contains(notification2.Id, SomePolymorphicNotificationHandler.Ids);
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

        await mediator.Publish((object)notification2);
        Assert.Contains(notification2.Id, SomePolymorphicNotificationHandler.Ids);
    }
}
