using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class NonSyncNotificationHandlerTests
{
    public sealed record SomeNonSyncNotification(Guid Id) : INotification;

    public sealed class SomeNonSyncNotificationHandler0 : INotificationHandler<SomeNonSyncNotification>
    {
        internal readonly ConcurrentDictionary<Guid, (int Count, long Timestamp)> InstanceIds = new();

        public async ValueTask Handle(SomeNonSyncNotification notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            InstanceIds.AddOrUpdate(
                notification.Id,
                (1, Stopwatch.GetTimestamp()),
                (_, data) => (data.Count + 1, Stopwatch.GetTimestamp())
            );
        }
    }

    public sealed class SomeNonSyncNotificationHandler1 : INotificationHandler<SomeNonSyncNotification>
    {
        internal readonly ConcurrentDictionary<Guid, (int Count, long Timestamp)> InstanceIds = new();

        public async ValueTask Handle(SomeNonSyncNotification notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            InstanceIds.AddOrUpdate(
                notification.Id,
                (1, Stopwatch.GetTimestamp()),
                (_, data) => (data.Count + 1, Stopwatch.GetTimestamp())
            );
        }
    }

    [Fact]
    public async Task Test_NonSync_Notification_Handlers()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNonSyncNotificationHandler0>();
        var handler2 = sp.GetRequiredService<SomeNonSyncNotificationHandler1>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        var timestampBefore = Stopwatch.GetTimestamp();
        await mediator.Publish(new SomeNonSyncNotification(id), TestContext.Current.CancellationToken);
        var timestampAfter = Stopwatch.GetTimestamp();

        AssertInstanceIdCount(1, handler1.InstanceIds, id, timestampBefore, timestampAfter);
        AssertInstanceIdCount(1, handler2.InstanceIds, id, timestampBefore, timestampAfter);
    }
}
