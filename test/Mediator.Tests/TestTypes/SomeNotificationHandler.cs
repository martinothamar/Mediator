using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed class SomeNotificationHandler : INotificationHandler<SomeNotification>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();

    public ValueTask Handle(SomeNotification notification, CancellationToken cancellationToken)
    {
        Ids.Add(notification.Id);
        return default;
    }
}

public sealed class SomeStructNotificationHandler : INotificationHandler<SomeStructNotification>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();
    internal static readonly ConcurrentBag<long> Addresses = new();

    unsafe public ValueTask Handle(SomeStructNotification notification, CancellationToken cancellationToken)
    {
        var addr = *(long*)&notification;
        Ids.Add(notification.Id);
        return default;
    }
}
