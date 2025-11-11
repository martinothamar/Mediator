using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class NonSyncNotificationHandlerWithExceptionTests
{
    public sealed record SomeNonSyncNotification(Guid Id) : INotification;

    public sealed class SomeNonSyncNotificationHandler0 : INotificationHandler<SomeNonSyncNotification>
    {
        internal static bool ShouldThrow = false;
        internal readonly ConcurrentDictionary<Guid, (int Count, long Timestamp)> InstanceIds = new();

        public async ValueTask Handle(SomeNonSyncNotification notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            InstanceIds.AddOrUpdate(
                notification.Id,
                (1, Stopwatch.GetTimestamp()),
                (_, data) => (data.Count + 1, Stopwatch.GetTimestamp())
            );
            if (ShouldThrow)
                throw new Exception("marker0");
        }
    }

    public sealed class SomeNonSyncNotificationHandler1 : INotificationHandler<SomeNonSyncNotification>
    {
        internal static bool ShouldThrow = false;
        internal readonly ConcurrentDictionary<Guid, (int Count, long Timestamp)> InstanceIds = new();

        public async ValueTask Handle(SomeNonSyncNotification notification, CancellationToken cancellationToken)
        {
            await Task.Yield();
            InstanceIds.AddOrUpdate(
                notification.Id,
                (1, Stopwatch.GetTimestamp()),
                (_, data) => (data.Count + 1, Stopwatch.GetTimestamp())
            );
            if (ShouldThrow)
                throw new Exception("marker1");
        }
    }

    [Fact]
    public async Task Test_NonSync_Notification_Handlers_With_AggregateException_0()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNonSyncNotificationHandler0>();
        var handler2 = sp.GetRequiredService<SomeNonSyncNotificationHandler1>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        SomeNonSyncNotificationHandler0.ShouldThrow = true;
        var timestampBefore = Stopwatch.GetTimestamp();
        var ct = TestContext.Current.CancellationToken;
        var ex = await Assert.ThrowsAsync<AggregateException>(async () =>
            await mediator.Publish(new SomeNonSyncNotification(id), ct)
        );
        var timestampAfter = Stopwatch.GetTimestamp();
        Assert.NotNull(ex);
        ex.InnerExceptions.Count.Should().Be(1);
        var innerEx = ex.InnerExceptions[0];
        Assert.True(innerEx is Exception);
        Assert.Equal("marker0", innerEx.Message);
        SomeNonSyncNotificationHandler0.ShouldThrow = false;

        AssertInstanceIdCount(1, handler1.InstanceIds, id, timestampBefore, timestampAfter);
        AssertInstanceIdCount(1, handler2.InstanceIds, id, timestampBefore, timestampAfter);
    }

    [Fact]
    public async Task Test_NonSync_Notification_Handlers_With_AggregateException_1()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNonSyncNotificationHandler0>();
        var handler2 = sp.GetRequiredService<SomeNonSyncNotificationHandler1>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        SomeNonSyncNotificationHandler1.ShouldThrow = true;
        var timestampBefore = Stopwatch.GetTimestamp();
        var ct = TestContext.Current.CancellationToken;
        var ex = await Assert.ThrowsAsync<AggregateException>(async () =>
            await mediator.Publish(new SomeNonSyncNotification(id), ct)
        );
        var timestampAfter = Stopwatch.GetTimestamp();
        Assert.NotNull(ex);
        ex.InnerExceptions.Count.Should().Be(1);
        var innerEx = ex.InnerExceptions[0];
        Assert.True(innerEx is Exception);
        Assert.Equal("marker1", innerEx.Message);
        SomeNonSyncNotificationHandler1.ShouldThrow = false;

        AssertInstanceIdCount(1, handler1.InstanceIds, id, timestampBefore, timestampAfter);
        AssertInstanceIdCount(1, handler2.InstanceIds, id, timestampBefore, timestampAfter);
    }
}
