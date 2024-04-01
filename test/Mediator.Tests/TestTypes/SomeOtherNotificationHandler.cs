using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed class SomeOtherNotificationHandler : INotificationHandler<SomeNotification>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();
    internal readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

    public ValueTask Handle(SomeNotification notification, CancellationToken cancellationToken)
    {
        Ids.Add(notification.Id);
        InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
        return default;
    }
}
