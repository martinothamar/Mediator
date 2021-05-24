using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes
{
    public interface ISomeNotification : INotification
    {
        Guid Id { get; }
    }

    public sealed record SomeNotification(Guid Id) : ISomeNotification;

    public sealed record SomeOtherNotification(Guid Id) : ISomeNotification;

    public sealed record SomeNotificationWithoutConcreteHandler(Guid Id) : INotification;

    public sealed class CatchAllPolymorphicNotificationHandler : INotificationHandler<INotification>
    {
        public static Guid Id { get; private set; }

        public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
        {
            if (notification is SomeNotificationWithoutConcreteHandler n)
                Id = n.Id;
            return default;
        }
    }

    public sealed class SomeGenericConstrainedNotificationHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : ISomeNotification
    {
        public static Guid Id { get; private set; }

        public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
        {
            Id = notification.Id;
            return default;
        }
    }
}
