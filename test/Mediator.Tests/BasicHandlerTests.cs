using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>;
    public sealed record SomeResponse(Guid Id);
    public sealed class SomeRequestHandler : IRequestHandler<SomeRequest, SomeResponse>
    {
        public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken) => new ValueTask<SomeResponse>(new SomeResponse(request.Id));
    }

    public sealed record SomeQuery(Guid Id) : IQuery<SomeResponse>;
    public sealed class SomeQueryHandler : IQueryHandler<SomeQuery, SomeResponse>
    {
        public ValueTask<SomeResponse> Handle(SomeQuery Query, CancellationToken cancellationToken) => new ValueTask<SomeResponse>(new SomeResponse(Query.Id));
    }

    public sealed record SomeCommand(Guid Id) : ICommand;
    public sealed class SomeCommandHandler : ICommandHandler<SomeCommand>
    {
        internal Guid Id;

        public ValueTask Handle(SomeCommand command, CancellationToken cancellationToken)
        {
            Id = command.Id;
            return default;
        }
    }

    public sealed record SomeNotification(Guid Id) : INotification;
    public sealed class SomeNotificationHandler : INotificationHandler<SomeNotification>
    {
        internal Guid Id = default;

        public ValueTask Handle(SomeNotification Notification, CancellationToken cancellationToken)
        {
            Id = Notification.Id;
            return default;
        }
    }

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
            await mediator.Send(new SomeCommand(id));
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
    }
}
