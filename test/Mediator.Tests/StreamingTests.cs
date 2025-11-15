using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;

namespace Mediator.Tests;

public sealed class StreamingTests
{
    public static IEnumerable<object[]> TestMessages = new IStreamMessage[][]
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
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in sender.CreateStream(new SomeStreamingQuery(id), ct))
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
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        int counter = 0;
        await foreach (var response in sender.CreateStream(message, ct))
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
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id), ct))
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
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();

        int counter = 0;
        await foreach (var response in mediator.CreateStream(message, ct))
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
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();

        var stream = message switch
        {
            SomeStreamingRequest m => mediator.CreateStream(m, ct),
            SomeStreamingCommand m => mediator.CreateStream(m, ct),
            SomeStreamingQuery m => mediator.CreateStream(m, ct),
            SomeStreamingCommandStruct m => mediator.CreateStream(m, ct),
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
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id), ct).WithCancellation(token))
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
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.CreateStream((IStreamQuery<SomeResponse>)null!, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.CreateStream(null!, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await concrete.CreateStream((SomeStreamingQuery)null!, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamQuery_Handler_NonNull_NonQuery()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () =>
            await mediator.CreateStream(message, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamQuery_NonNull_NoHandler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var query = new SomeStreamingQueryWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.CreateStream((object)query, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.CreateStream(query, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await concrete.CreateStream(query, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamCommand_Handler_Null_Input()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.CreateStream((IStreamCommand<SomeResponse>)null!, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.CreateStream(null!, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await concrete.CreateStream((SomeStreamingCommand)null!, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamCommand_Handler_NonNull_NonCommand()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () =>
            await mediator.CreateStream(message, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamCommand_NonNull_NoHandler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var command = new SomeStreamingCommandWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.CreateStream((object)command, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.CreateStream(command, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await concrete.CreateStream(command, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamRequest_Handler_Null_Input()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.CreateStream((IStreamRequest<SomeResponse>)null!, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await mediator.CreateStream(null!, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await concrete.CreateStream((SomeStreamingRequest)null!, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamRequest_Handler_NonNull_NonRequest()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        object message = new { Id = id };

        await Assert.ThrowsAsync<InvalidMessageException>(async () =>
            await mediator.CreateStream(message, ct).ToListAsync(ct)
        );
    }

    [Fact]
    public async Task Test_StreamRequest_NonNull_NoHandler()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var request = new SomeStreamingRequestWithoutHandler(id);

        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.CreateStream((object)request, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await mediator.CreateStream(request, ct).ToListAsync(ct)
        );
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () =>
            await concrete.CreateStream(request, ct).ToListAsync(ct)
        );
    }
}
