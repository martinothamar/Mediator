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
            Assert.Equal(id, requestHandler.Id);
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
            Assert.Equal(id, commandHandler.Id);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
        }

        [Fact]
        public async Task Test_CommandWithoutResponse_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var commandHandler = sp.GetRequiredService<SomeCommandWithoutResponseHandler>();
            await mediator.Send(new SomeCommandWithoutResponse(id));
            Assert.Equal(id, commandHandler.Id);
        }

        [Fact]
        public async Task Test_StructCommand_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var commandHandler = sp.GetRequiredService<SomeStructCommandHandler>();
            await mediator.Send(new SomeStructCommand(id));
            Assert.Equal(id, commandHandler.Id);
        }

        [Fact]
        public async Task Test_Notification_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var commandHandler = sp.GetRequiredService<SomeNotificationHandler>();
            await mediator.Publish(new SomeNotification(id));
            Assert.Equal(id, commandHandler.Id);
        }

        [Fact]
        public async Task Test_Multiple_Notification_Handlers()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
            var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
            await mediator.Publish(new SomeNotification(id));
            Assert.Equal(id, handler1.Id);
            Assert.Equal(id, handler2.Id);
        }

        [Fact]
        public async Task Test_Static_Nested_Request_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var handler = sp.GetRequiredService<SomeStaticClass.SomeStaticNestedHandler>();
            var response = await mediator!.Send(new SomeStaticClass.SomeStaticNestedRequest(id), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
            Assert.Equal(id, handler.Id);
        }

        [Fact]
        public async Task Test_Nested_Request_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            var handler = sp.GetRequiredService<SomeOtherClass.SomeNestedHandler>();
            var response = await mediator!.Send(new SomeOtherClass.SomeNestedRequest(id), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
            Assert.Equal(id, handler.Id);
        }

        [Fact]
        public async Task Test_Request_Without_Handler()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var request = new SomeRequestWithoutHandler();

            var handler = sp.GetService<SomeAbstractRequestHandler>();
            Assert.Null(handler);

            await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await mediator.Send(request));
        }
    }
}
