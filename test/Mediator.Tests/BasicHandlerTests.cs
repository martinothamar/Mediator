using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

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
    public async Task Test_Request_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeRequest(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeRequest(id)) as SomeResponse;
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeRequest(id));
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Request_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send((IRequest<SomeResponse>)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Send((SomeRequest)null!));
    }

    [Fact]
    public async Task Test_RequestWithoutResponse_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var requestHandler = sp.GetRequiredService<SomeRequestWithoutResponseHandler>();
        await mediator!.Send(new SomeRequestWithoutResponse(id));
        Assert.NotNull(requestHandler);
        Assert.Contains(id, SomeRequestWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_Query_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeQuery(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeQuery(id)) as SomeResponse;
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeQuery(id));
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Query_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send((IQuery<SomeResponse>)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Send((SomeQuery)null!));
    }

    [Fact]
    public async Task Test_Command_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var commandHandler = sp.GetRequiredService<SomeCommandHandler>();
        var response = await mediator.Send(new SomeCommand(id));
        Assert.NotNull(commandHandler);
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        response = await mediator.Send((object)new SomeCommand(id)) as SomeResponse;
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
        response = await concrete.Send(new SomeCommand(id));
        Assert.Contains(id, SomeCommandHandler.Ids);
        Assert.NotNull(response);
        Assert.Equal(id, response?.Id);
    }

    [Fact]
    public async Task Test_Command_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send((ICommand<SomeResponse>)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Send(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Send((SomeCommand)null!));
    }

    [Fact]
    public async Task Test_CommandWithoutResponse_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var commandHandler = sp.GetRequiredService<SomeCommandWithoutResponseHandler>();
        Assert.NotNull(commandHandler);
        await mediator.Send(new SomeCommandWithoutResponse(id));
        Assert.Contains(id, SomeCommandWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_StructCommand_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var command = new SomeStructCommand(id);

        var commandHandler = sp.GetRequiredService<SomeStructCommandHandler>();
        Assert.NotNull(commandHandler);
        await mediator.Send(command);
        Assert.Contains(id, SomeStructCommandHandler.Ids);
        await concrete.Send(in command);
    }

    [Fact]
    public async Task Test_Notification_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await mediator.Publish(new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
    }

    [Fact]
    public async Task Test_INotification_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await mediator.Publish<INotification>(new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
    }

    [Fact]
    public async Task Test_Notification_Handler_Null_Input()
    {
        var (sp, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var notificationHandler = sp.GetRequiredService<SomeNotificationHandler>();
        Assert.NotNull(notificationHandler);
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish<INotification>(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish<SomeNotification>(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await concrete.Publish((SomeNotification)null!));
        Assert.DoesNotContain(id, SomeNotificationHandler.Ids);
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
        await mediator.Publish((object)new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
    }

    [Fact]
    public async Task Test_Multiple_Object_Notification_Handlers()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler1 = sp.GetRequiredService<SomeNotificationHandler>();
        var handler2 = sp.GetRequiredService<SomeOtherNotificationHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        await mediator.Publish((object)new SomeNotification(id));
        Assert.Contains(id, SomeNotificationHandler.Ids);
        Assert.Contains(id, SomeOtherNotificationHandler.Ids);
    }

    [Fact]
    public async Task Test_Static_Nested_Request_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<SomeStaticClass.SomeStaticNestedHandler>();
        var response = await mediator!.Send(new SomeStaticClass.SomeStaticNestedRequest(id));
        Assert.NotNull(handler);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Contains(id, SomeStaticClass.SomeStaticNestedHandler.Ids);
    }

    [Fact]
    public async Task Test_Nested_Request_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<SomeOtherClass.SomeNestedHandler>();
        var response = await mediator!.Send(new SomeOtherClass.SomeNestedRequest(id));
        Assert.NotNull(handler);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Contains(id, SomeOtherClass.SomeNestedHandler.Ids);
    }
}
