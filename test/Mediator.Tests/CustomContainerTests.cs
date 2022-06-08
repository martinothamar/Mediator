using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediator.Tests;

public class CustomContainerTests
{
    [Fact]
    public void Test_Init()
    {
        var (_, mediator) = Fixture.GetMediatorCustomContainer();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void Test_Container_Returns_Lists()
    {
        var (sp, _) = Fixture.GetMediatorCustomContainer();
        var handlers = sp.GetServices<INotificationHandler<SomeStructNotification>>();
        Assert.True(typeof(List<INotificationHandler<SomeStructNotification>>) == handlers.GetType());
    }

    [Fact]
    public async Task Test_Notifications()
    {
        var (_, mediator) = Fixture.GetMediatorCustomContainer();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var notification = new SomeStructNotification(id);

        await mediator.Publish(notification);
        Assert.Contains(id, SomeStructNotificationHandler.Ids);
        SomeStructNotificationHandler.Ids.Clear();
        Assert.DoesNotContain(id, SomeStructNotificationHandler.Ids);

        await concrete.Publish(notification);
        Assert.Contains(id, SomeStructNotificationHandler.Ids);
    }

    [Fact]
    public async Task Test_Request_Handler()
    {
        var (_, mediator) = Fixture.GetMediatorCustomContainer();
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
    public async Task Test_Query_Handler()
    {
        var (_, mediator) = Fixture.GetMediatorCustomContainer();
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
    public async Task Test_Command_Handler()
    {
        var (sp, mediator) = Fixture.GetMediatorCustomContainer();
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
}
