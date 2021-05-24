using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public sealed class AbstractNotificationHandlerTests
    {
        public sealed record NotificationWithAbstractHandler(Guid Id) : INotification;

        public abstract class AbstractNotificationHandler : INotificationHandler<NotificationWithAbstractHandler>
        {
            public abstract ValueTask Handle(NotificationWithAbstractHandler notification, CancellationToken cancellationToken);
        }

        public sealed class ConcreteNotificationHandler : AbstractNotificationHandler
        {
            internal static Guid Id;

            public override ValueTask Handle(NotificationWithAbstractHandler notification, CancellationToken cancellationToken)
            {
                Id = notification.Id;
                return default;
            }
        }

        public sealed class ConcreteNotificationHandler2 : AbstractNotificationHandler
        {
            internal static Guid Id;

            public override ValueTask Handle(NotificationWithAbstractHandler notification, CancellationToken cancellationToken)
            {
                Id = notification.Id;
                return default;
            }
        }

        [Fact]
        public async Task Test_Succeeds()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            await mediator.Publish(new NotificationWithAbstractHandler(id));

            var handler1 = sp.GetRequiredService<ConcreteNotificationHandler>();
            var handler2 = sp.GetRequiredService<ConcreteNotificationHandler2>();
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.Equal(id, ConcreteNotificationHandler.Id);
            Assert.Equal(id, ConcreteNotificationHandler2.Id);
        }
    }
}
