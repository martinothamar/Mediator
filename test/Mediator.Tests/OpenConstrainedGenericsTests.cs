using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public sealed class OpenConstrainedGenericsTests
    {
        [Fact]
        public async Task Test_Constrained_Generic_Argument_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var notification1 = new SomeNotification(Guid.NewGuid());
            var notification2 = new SomeOtherNotification(Guid.NewGuid());

            var handler1 = sp.GetRequiredService<SomeGenericConstrainedNotificationHandler<SomeNotification>>();
            var handler2 = sp.GetRequiredService<SomeGenericConstrainedNotificationHandler<SomeOtherNotification>>();

            await mediator.Publish(notification1);
            Assert.Equal(notification1.Id, handler1.Id);

            await mediator.Publish(notification2);
            Assert.Equal(notification2.Id, handler2.Id);
        }
    }
}
