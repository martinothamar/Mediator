using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
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
            public static Guid Id { get; private set; }

            public ValueTask Handle(IPolymorphicDispatchNotification notification, CancellationToken cancellationToken)
            {
                Id = notification.Id;
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
            Assert.Equal(default, SomePolymorphicNotificationHandler.Id);

            await mediator.Publish(notification1);
            Assert.Equal(notification1.Id, SomePolymorphicNotificationHandler.Id);

            await mediator.Publish(notification2);
            Assert.Equal(notification2.Id, SomePolymorphicNotificationHandler.Id);
        }
    }
}
