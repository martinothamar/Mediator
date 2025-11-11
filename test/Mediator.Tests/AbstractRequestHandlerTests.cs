using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public sealed class AbstractNotificationHandlerTests
{
    public sealed record NotificationWithAbstractHandler(Guid Id) : INotification;

    public abstract class AbstractNotificationHandler : INotificationHandler<NotificationWithAbstractHandler>
    {
        public abstract ValueTask Handle(
            NotificationWithAbstractHandler notification,
            CancellationToken cancellationToken
        );
    }

    public sealed class ConcreteNotificationHandler : AbstractNotificationHandler
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();
        internal readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public override ValueTask Handle(
            NotificationWithAbstractHandler notification,
            CancellationToken cancellationToken
        )
        {
            Ids.Add(notification.Id);
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }
    }

    public sealed class ConcreteNotificationHandler2 : AbstractNotificationHandler
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();
        internal readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public override ValueTask Handle(
            NotificationWithAbstractHandler notification,
            CancellationToken cancellationToken
        )
        {
            Ids.Add(notification.Id);
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }
    }

    [Fact]
    public async Task Test_Succeeds()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        await mediator.Publish(new NotificationWithAbstractHandler(id), TestContext.Current.CancellationToken);

        var handler1 = sp.GetRequiredService<ConcreteNotificationHandler>();
        var handler2 = sp.GetRequiredService<ConcreteNotificationHandler2>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.Contains(id, ConcreteNotificationHandler.Ids);
        Assert.Contains(id, ConcreteNotificationHandler2.Ids);
        AssertInstanceIdCount(1, handler1.InstanceIds, id);
        AssertInstanceIdCount(1, handler2.InstanceIds, id);
    }
}
