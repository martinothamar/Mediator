using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public sealed class PolymorphicDispatchTests
    {
        [Fact]
        public async Task Test_Multiple_Notification_Handlers()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var notification1 = new SomeNotification(Guid.NewGuid());
            var notification2 = new SomeOtherNotification(Guid.NewGuid());

            var handler = sp.GetRequiredService<SomePolymorphicNotificationHandler>();

            await mediator.Publish(notification1);
            Assert.Equal(notification1.Id, handler.Id);

            await mediator.Publish(notification2);
            Assert.Equal(notification2.Id, handler.Id);
        }
    }
}
