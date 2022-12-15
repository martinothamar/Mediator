using Mediator.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests;

public sealed class StreamingTests
{
    public static IEnumerable<IStreamMessage[]> TestMessages = new IStreamMessage[][]
    {
        new IStreamMessage[] { new SomeStreamingRequest(Guid.NewGuid()) },
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

    [Fact]
    public async Task Test_StreamQuery_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await mediator.CreateStream((IStreamQuery<SomeResponse>)null!).ToListAsync()
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.CreateStream(null!).ToListAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await concrete.CreateStream((SomeStreamingQuery)null!).ToListAsync()
        );
    }

    [Fact]
    public async Task Test_StreamQuery_Handler_NonNull_NonQuery()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };
        var query = Unsafe.As<object, IStreamQuery<SomeResponse>>(ref message);

        await Assert.ThrowsAsync<InvalidMessageException>(
            async () => await mediator.CreateStream(message).ToListAsync()
        );
        await Assert.ThrowsAsync<InvalidMessageException>(async () => await mediator.CreateStream(query).ToListAsync());
    }

    [Fact]
    public async Task Test_StreamQuery_NonNull_NoHandler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var query = new SomeStreamingQueryWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await mediator.CreateStream((object)query).ToListAsync()
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await mediator.CreateStream(query).ToListAsync()
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await concrete.CreateStream(query).ToListAsync()
        );
    }

    [Fact]
    public async Task Test_StreamCommand_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await mediator.CreateStream((IStreamCommand<SomeResponse>)null!).ToListAsync()
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.CreateStream(null!).ToListAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await concrete.CreateStream((SomeStreamingCommand)null!).ToListAsync()
        );
    }

    [Fact]
    public async Task Test_StreamCommand_Handler_NonNull_NonCommand()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };
        var command = Unsafe.As<object, IStreamCommand<SomeResponse>>(ref message);

        await Assert.ThrowsAsync<InvalidMessageException>(
            async () => await mediator.CreateStream(message).ToListAsync()
        );
        await Assert.ThrowsAsync<InvalidMessageException>(
            async () => await mediator.CreateStream(command).ToListAsync()
        );
    }

    [Fact]
    public async Task Test_StreamCommand_NonNull_NoHandler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var command = new SomeStreamingCommandWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await mediator.CreateStream((object)command).ToListAsync()
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await mediator.CreateStream(command).ToListAsync()
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await concrete.CreateStream(command).ToListAsync()
        );
    }

    [Fact]
    public async Task Test_StreamRequest_Handler_Null_Input()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await mediator.CreateStream((IStreamRequest<SomeResponse>)null!).ToListAsync()
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.CreateStream(null!).ToListAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await concrete.CreateStream((SomeStreamingRequest)null!).ToListAsync()
        );
    }

    [Fact]
    public async Task Test_StreamRequest_Handler_NonNull_NonRequest()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };
        var request = Unsafe.As<object, IStreamRequest<SomeResponse>>(ref message);

        await Assert.ThrowsAsync<InvalidMessageException>(
            async () => await mediator.CreateStream(message).ToListAsync()
        );
        await Assert.ThrowsAsync<InvalidMessageException>(
            async () => await mediator.CreateStream(request).ToListAsync()
        );
    }

    [Fact]
    public async Task Test_StreamRequest_NonNull_NoHandler()
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var request = new SomeStreamingRequestWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await mediator.CreateStream((object)request).ToListAsync()
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await mediator.CreateStream(request).ToListAsync()
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            async () => await concrete.CreateStream(request).ToListAsync()
        );
    }
}
