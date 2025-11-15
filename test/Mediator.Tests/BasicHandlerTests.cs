using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using static Mediator.Tests.OpenConstrainedGenericsTests;

namespace Mediator.Tests;

public class BasicHandlerTests
{
    [Fact]
    public void Test_Initialization()
    {
        var (_, mediator) = Fixture.GetMediator();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void Test_Get_Handlers()
    {
        var (sp, _) = Fixture.GetMediator();

        INotificationHandler<SomeStructNotification> handler1 = new SomeStructNotificationHandler();
        //INotificationHandler<SomeStructNotification> handler2 = new CatchAllPolymorphicNotificationHandler();
        INotificationHandler<SomeStructNotification> handler3 =
            new SomeGenericConstrainedNotificationHandler<SomeStructNotification>();

        var handlers = sp.GetServices<INotificationHandler<SomeStructNotification>>();
        Assert.NotNull(handlers);
        var handlersArray = handlers.ToArray();
        Assert.NotNull(handlersArray);
    }

    [Fact]
    public async Task Test_Request_Handler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeRequest(id), cancellationToken);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeRequest(id), cancellationToken) as SomeResponse;
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeRequest(id), cancellationToken);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Request_Handler_Null_Input()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.Send((IRequest<SomeResponse>)null!, cancellationToken)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await concrete.Send((SomeRequest)null!, cancellationToken)
        );
    }

    [Fact]
    public async Task Test_Request_Handler_NonNull_NonRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Send(message, cancellationToken));
    }

    [Fact]
    public async Task Test_Request_NonNull_NoHandler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var request = new SomeRequestWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.Send((object)request, cancellationToken)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.Send(request, cancellationToken)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await concrete.Send(request, cancellationToken)
        );
    }

    [Fact]
    public async Task Test_RequestWithoutResponse_Handler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var requestHandler = sp.GetRequiredService<IRequestHandler<SomeRequestWithoutResponse, Unit>>();
        await mediator!.Send(new SomeRequestWithoutResponse(id), cancellationToken);
        Assert.NotNull(requestHandler);
        Assert.Contains(id, SomeRequestWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_Query_Handler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeQuery(id), cancellationToken);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeQuery(id), cancellationToken) as SomeResponse;
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeQuery(id), cancellationToken);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Query_Handler_Null_Input()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.Send((IQuery<SomeResponse>)null!, cancellationToken)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await concrete.Send((SomeQuery)null!, cancellationToken)
        );
    }

    [Fact]
    public async Task Test_Query_Handler_NonNull_NonQuery()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Send(message, ct));
    }

    [Fact]
    public async Task Test_Query_NonNull_NoHandler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var query = new SomeQueryWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.Send((object)query, cancellationToken)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.Send(query, cancellationToken)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await concrete.Send(query, cancellationToken)
        );
    }

    [Fact]
    public async Task Test_Command_Handler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var commandHandler = sp.GetRequiredService<ICommandHandler<SomeCommand, SomeResponse>>();
        var response = await mediator.Send(new SomeCommand(id), cancellationToken);
        Assert.NotNull(commandHandler);
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeCommand(id), cancellationToken) as SomeResponse;
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeCommand(id), cancellationToken);
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Command_Handler_Null_Input()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.Send((ICommand<SomeResponse>)null!, ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Send((SomeCommand)null!, ct));
    }

    [Fact]
    public async Task Test_Command_Handler_NonNull_NonCommand()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Send(message, ct));
    }

    [Fact]
    public async Task Test_Command_NonNull_NoHandler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var command = new SomeCommandWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.Send((object)command, cancellationToken)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.Send(command, cancellationToken)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await concrete.Send(command, cancellationToken)
        );
    }

    [Fact]
    public async Task Test_CommandWithoutResponse_Handler()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var commandHandler = sp.GetRequiredService<ICommandHandler<SomeCommandWithoutResponse, Unit>>();
        Assert.NotNull(commandHandler);
        await mediator.Send(new SomeCommandWithoutResponse(id), cancellationToken);
        Assert.Contains(id, SomeCommandWithoutResponseHandler.Ids);
    }

    [Fact]
    public unsafe void Test_StructCommand_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var command = new SomeStructCommand(id);
        var addr = *(long*)&command;

        var commandHandler = sp.GetRequiredService<ICommandHandler<SomeStructCommand, Unit>>();
        Assert.NotNull(commandHandler);
#pragma warning disable xUnit1031
        mediator.Send(command, ct).GetAwaiter().GetResult();
#pragma warning restore xUnit1031
        Assert.Contains(id, SomeStructCommandHandler.Ids);
        Assert.Contains(addr, SomeStructCommandHandler.Addresses);
    }

    [Fact]
    public unsafe void Test_StructCommand_Handler_Concrete()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var command = new SomeStructCommand(id);
        var addr = *(long*)&command;

        var commandHandler = sp.GetRequiredService<ICommandHandler<SomeStructCommand, Unit>>();
#pragma warning disable xUnit1031
        concrete.Send(command, ct).GetAwaiter().GetResult();
#pragma warning restore xUnit1031
        Assert.Contains(id, SomeStructCommandHandler.Ids);
        Assert.Contains(addr, SomeStructCommandHandler.Addresses);
    }

    [Fact]
    public async Task Test_Notification_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await mediator.Publish(new SomeNotification(id), ct);
        Assert.Contains(id, SomeNotificationHandler.Ids);
        AssertInstanceIdCount(1, notificationHandler.InstanceIds, id);

        var handlers = sp.GetServices<INotificationHandler<SomeNotification>>();
        Assert.True(handlers.Distinct().Count() == handlers.Count());
    }

    [Fact]
    public async Task Test_Struct_Notification_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var notification = new SomeStructNotification(id);

        var handlers = sp.GetServices<INotificationHandler<SomeStructNotification>>();
        Assert.True(handlers.Count() == 2);
        Assert.True(handlers.Distinct().Count() == handlers.Count());

        var notificationHandler = sp.GetRequiredService<SomeStructNotificationHandler>();
        Assert.NotNull(notificationHandler);

        await concrete.Publish(notification, ct);
        Assert.Contains(id, SomeStructNotificationHandler.Ids);
        AssertInstanceIdCount(1, notificationHandler.InstanceIds, id);
    }

    [Fact]
    public async Task Test_INotification_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await mediator.Publish<INotification>(new SomeNotification(id), ct);
        Assert.Contains(id, SomeNotificationHandler.Ids);
        AssertInstanceIdCount(1, notificationHandler.InstanceIds, id);

        var handlers = sp.GetServices<INotificationHandler<SomeNotification>>();
        Assert.True(handlers.Distinct().Count() == handlers.Count());
    }

    [Fact]
    public async Task Test_Notification_Handler_Null_Input()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish<INotification>(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.Publish<SomeNotification>(null!, ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish(null!, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await concrete.Publish((SomeNotification)null!, ct)
        );
        Assert.DoesNotContain(id, SomeNotificationHandler.Ids);
        AssertInstanceIdCount(0, notificationHandler.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Notification_Handler_NonNull_NonNotification()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.Publish(message, ct));
    }

    [Fact]
    public async Task Test_Multiple_Notification_Handlers()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
        var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        await mediator.Publish(new SomeNotification(id), ct);
        await mediator.Publish((object)new SomeNotification(id), ct);
        Assert.Contains(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
        AssertInstanceIdCount(2, handler1.InstanceIds, id);
        AssertInstanceIdCount(2, handler2.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Multiple_Object_Notification_Handlers()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
        var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        await mediator.Publish((object)new SomeNotification(id), ct);
        Assert.Contains(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler1.InstanceIds, id);
        AssertInstanceIdCount(1, handler2.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Static_Nested_Request_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<IRequestHandler<SomeStaticClass.SomeStaticNestedRequest, SomeResponse>>();
        var response = await mediator!.Send(new SomeStaticClass.SomeStaticNestedRequest(id), ct);
        Assert.NotNull(handler);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Contains(id, SomeStaticClass.SomeStaticNestedHandler.Ids);
    }

    [Fact]
    public async Task Test_Nested_Request_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<IRequestHandler<SomeOtherClass.SomeNestedRequest, SomeResponse>>();
        var response = await mediator!.Send(new SomeOtherClass.SomeNestedRequest(id), ct);
        Assert.NotNull(handler);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Contains(id, SomeOtherClass.SomeNestedHandler.Ids);
    }

    [Fact]
    public async Task Test_Request_Returning_Array()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();
        var bytes = id.ToByteArray();

        var request = new SomeRequestReturningByteArray(id);
        var receivedBytes = await mediator.Send(request, ct);
        Assert.Equal(bytes, receivedBytes);
    }

    [Fact]
    public async Task Test_Remove_NotificationHandler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(sc =>
        {
            var sds = sc.Where(static a =>
                a.ImplementationFactory is { } implFac
                && implFac.Method.IsGenericMethod
                && implFac.Method.GetGenericArguments().Any(static b => b.Name == nameof(SomeNotificationHandler))
            );
            var sd = Assert.Single(sds);
            sc.Remove(sd);
        });

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
        var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        await mediator.Publish(new SomeNotification(id), ct);
        await mediator.Publish((object)new SomeNotification(id), ct);
        Assert.DoesNotContain(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
        AssertInstanceIdCount(0, handler1.InstanceIds, id);
        AssertInstanceIdCount(2, handler2.InstanceIds, id);
    }
}
