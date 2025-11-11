using System;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public sealed class SenderTests
{
    [Fact]
    public async Task Test_Request_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var response = await sender.Send(new SomeRequest(id), ct);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
    }

    [Fact]
    public async Task Test_RequestWithoutResponse_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<IRequestHandler<SomeRequestWithoutResponse, Unit>>();
        Assert.NotNull(handler);
        await sender.Send(new SomeRequestWithoutResponse(id), ct);
        Assert.Contains(id, SomeRequestWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_Command_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var response = await sender.Send(new SomeCommand(id), ct);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
    }

    [Fact]
    public async Task Test_CommandWithoutResponse_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<ICommandHandler<SomeCommandWithoutResponse, Unit>>();
        Assert.NotNull(handler);
        await sender.Send(new SomeCommandWithoutResponse(id), ct);
        Assert.Contains(id, SomeCommandWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_Query_Handler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var response = await sender.Send(new SomeQuery(id), ct);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
    }
}
