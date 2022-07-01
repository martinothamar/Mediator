using Mediator.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests;

public sealed class StreamingTests
{
    public static IEnumerable<IStreamMessage[]> TestMessages = new IStreamMessage[][]
    {
        new IStreamMessage[] { new SomeStreamingQuery(Guid.NewGuid()) },
        new IStreamMessage[] { new SomeStreamingCommand(Guid.NewGuid()) },
        new IStreamMessage[] { new SomeStreamingQuery(Guid.NewGuid()) },
        new IStreamMessage[] { new SomeStreamingCommandStruct(Guid.NewGuid()) },
    };

    private static Guid GetId(IStreamMessage message) => (Guid)message.GetType().GetProperty("Id")!.GetValue(message)!;

    [Fact]
    public async Task Test_ISender()
    {
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in sender.CreateStream(new SomeStreamingQuery(id)))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    [Theory]
    [MemberData(nameof(TestMessages))]
    public async Task Test_ISender_Object(IStreamMessage message)
    {
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        int counter = 0;
        await foreach (var response in sender.CreateStream(message))
        {
            Assert.IsType<SomeResponse>(response);
            Assert.Equal(GetId(message), ((SomeResponse)response!).Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    [Fact]
    public async Task Test_IMediator()
    {
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id)))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    [Theory]
    [MemberData(nameof(TestMessages))]
    public async Task Test_IMediator_Object(IStreamMessage message)
    {
        var (_, mediator) = Fixture.GetMediator();

        int counter = 0;
        await foreach (var response in mediator.CreateStream(message))
        {
            Assert.IsType<SomeResponse>(response);
            Assert.Equal(GetId(message), ((SomeResponse)response!).Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    [Theory]
    [MemberData(nameof(TestMessages))]
    public async Task Test_IMediator_Concrete(IStreamMessage message)
    {
        var (_, mediator) = Fixture.GetMediator();

        var stream = message switch
        {
            SomeStreamingRequest m => mediator.CreateStream(m),
            SomeStreamingCommand m => mediator.CreateStream(m),
            SomeStreamingQuery m => mediator.CreateStream(m),
            SomeStreamingCommandStruct m => mediator.CreateStream(m),
            _ => throw new Exception("Invalid message"),
        };

        int counter = 0;
        await foreach (var response in stream)
        {
            Assert.Equal(GetId(message), response.Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    [Fact]
    public async Task Test_Cancellation_Parameter()
    {
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id), token))
        {
            Assert.Equal(id, response.Id);
            counter++;

            cts.Cancel();
        }

        Assert.Equal(1, counter);
    }

    [Fact]
    public async Task Test_Cancellation_WithCancellation_Method()
    {
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id)).WithCancellation(token))
        {
            Assert.Equal(id, response.Id);
            counter++;

            cts.Cancel();
        }

        Assert.Equal(1, counter);
    }
}
