using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public class BasicHandlerTests
    {
        [Fact]
        public void Test_Initialization()
        {
            var (_, mediator) = Fixture.GetMediator();
            Assert.NotNull(mediator);
        }

        [Fact]
        public async Task Test_Request_Handler()
        {
            var (_, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var response = await mediator!.Send(new SomeRequest(id), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
        }

        [Fact]
        public async Task Test_RequestWithoutResponse_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var requestHandler = sp.GetRequiredService<SomeRequestWithoutResponseHandler>();
            await mediator!.Send(new SomeRequestWithoutResponse(id), CancellationToken.None);
            Assert.NotNull(requestHandler);
            Assert.Equal(id, SomeRequestWithoutResponseHandler.Id);
        }

        [Fact]
        public async Task Test_Query_Handler()
        {
            var (_, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var response = await mediator!.Send(new SomeQuery(id), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
        }

        [Fact]
        public async Task Test_Command_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var commandHandler = sp.GetRequiredService<SomeCommandHandler>();
            var response = await mediator.Send(new SomeCommand(id));
            Assert.NotNull(commandHandler);
            Assert.Equal(id, SomeCommandHandler.Id);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
        }

        [Fact]
        public async Task Test_CommandWithoutResponse_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var commandHandler = sp.GetRequiredService<SomeCommandWithoutResponseHandler>();
            Assert.NotNull(commandHandler);
            await mediator.Send(new SomeCommandWithoutResponse(id));
            Assert.Equal(id, SomeCommandWithoutResponseHandler.Id);
        }

        [Fact]
        public async Task Test_StructCommand_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var commandHandler = sp.GetRequiredService<SomeStructCommandHandler>();
            Assert.NotNull(commandHandler);
            await mediator.Send(new SomeStructCommand(id));
            Assert.Equal(id, SomeStructCommandHandler.Id);
        }

        [Fact]
        public async Task Test_Notification_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
            Assert.NotNull(notificationHandler);
            await mediator.Publish(new SomeNotification(id));
            Assert.Equal(id, SomeNotificationHandler.Id);
        }

        [Fact]
        public async Task Test_Multiple_Notification_Handlers()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
            var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            await mediator.Publish(new SomeNotification(id));
            Assert.Equal(id, SomeNotificationHandler.Id);
            Assert.Equal(id, SomeOtherNotificationHandler.Id);
        }

        [Fact]
        public async Task Test_Static_Nested_Request_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var handler = sp.GetRequiredService<SomeStaticClass.SomeStaticNestedHandler>();
            var response = await mediator!.Send(new SomeStaticClass.SomeStaticNestedRequest(id), CancellationToken.None);
            Assert.NotNull(handler);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
            Assert.Equal(id, SomeStaticClass.SomeStaticNestedHandler.Id);
        }

        [Fact]
        public async Task Test_Nested_Request_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var handler = sp.GetRequiredService<SomeOtherClass.SomeNestedHandler>();
            var response = await mediator!.Send(new SomeOtherClass.SomeNestedRequest(id), CancellationToken.None);
            Assert.NotNull(handler);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
            Assert.Equal(id, SomeOtherClass.SomeNestedHandler.Id);
        }
    }
}
