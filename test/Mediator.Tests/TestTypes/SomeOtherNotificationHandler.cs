using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes;

public sealed class SomeOtherNotificationHandler : INotificationHandler<SomeNotification>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();

    public ValueTask Handle(SomeNotification notification, CancellationToken cancellationToken)
    {
        Ids.Add(notification.Id);
        return default;
    }
}
