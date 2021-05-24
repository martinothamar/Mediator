using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes
{
    public sealed class SomeNotificationHandler : INotificationHandler<SomeNotification>
    {
        internal static Guid Id = default;

        public ValueTask Handle(SomeNotification Notification, CancellationToken cancellationToken)
        {
            Id = Notification.Id;
            return default;
        }
    }
}
