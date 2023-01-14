using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline.ExceptionHandling;

public class MultipleHandlerTests
{
    [Fact]
    public async Task Test_ExceptionHandler()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<State>();
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionHandler<,>));
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionHandler2<,>));
            }
        );

        var id = Guid.NewGuid();

        var state = sp.GetRequiredService<State>();
        Assert.NotNull(state);
        state!.Reset();

        var response = await mediator.Send(new ErroringRequest(id, 1));
        Assert.Equal(id, response.Id);
        Assert.Equal(typeof(NotImplementedException), state.Handler1Exception?.GetType());
        Assert.Null(state.Handler2Exception);
        state.Reset();

        response = await mediator.Send(new ErroringRequest(id, 2));
        Assert.Equal(id, response.Id);
        Assert.Equal(typeof(NotImplementedException), state.Handler2Exception?.GetType());
        Assert.Null(state.Handler1Exception);
        state.Reset();

        await Assert.ThrowsAsync<NotImplementedException>(async () => await mediator.Send(new ErroringRequest(id, -1)));
        Assert.True(state.Handler1Timestamp > state.Handler2Timestamp);
        state.Reset();

        await Assert.ThrowsAsync<NotImplementedException>(async () => await mediator.Send(new ErroringRequest(id, -1)));
        Assert.True(state.Handler1Timestamp > state.Handler2Timestamp);
        state.Reset();
    }

    private sealed class State
    {
        public Exception? Handler1Exception { get; set; }
        public long Handler1Timestamp { get; set; }

        public Exception? Handler2Exception { get; set; }
        public long Handler2Timestamp { get; set; }

        public void Reset()
        {
            Handler1Exception = null;
            Handler2Exception = null;
            Handler1Timestamp = default;
            Handler2Timestamp = default;
        }
    }

    private sealed class ExceptionHandler<TMessage, TResponse> : MessageExceptionHandler<TMessage, TResponse>
        where TMessage : notnull, IMessage
        where TResponse : ICreateable<TResponse>
    {
        private readonly State _state;

        public ExceptionHandler(State state) => _state = state;

        protected override ValueTask<ExceptionHandlingResult<TResponse>> Handle(
            TMessage message,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            _state.Handler1Timestamp = Stopwatch.GetTimestamp();
            var id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            var n = (int)typeof(TMessage).GetProperty("N")!.GetValue(message)!;
            var handled = n is 1 or 3;
            if (handled)
            {
                _state.Handler1Exception = exception;
                return Handled(TResponse.Create(id));
            }
            else
            {
                return NotHandled;
            }
        }
    }

    private sealed class ExceptionHandler2<TMessage, TResponse> : MessageExceptionHandler<TMessage, TResponse>
        where TMessage : notnull, IMessage
        where TResponse : ICreateable<TResponse>
    {
        private readonly State _state;

        public ExceptionHandler2(State state) => _state = state;

        protected override ValueTask<ExceptionHandlingResult<TResponse>> Handle(
            TMessage message,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            _state.Handler2Timestamp = Stopwatch.GetTimestamp();
            var id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            var n = (int)typeof(TMessage).GetProperty("N")!.GetValue(message)!;
            var handled = n is 2 or 3 ? true : false;
            if (handled)
            {
                _state.Handler2Exception = exception;
                return Handled(TResponse.Create(id));
            }
            else
            {
                return NotHandled;
            }
        }
    }

    public interface ICreateable<TResponse> where TResponse : ICreateable<TResponse>
    {
        static abstract TResponse Create(Guid id);
    }

    public sealed record TestResponse(Guid Id) : ICreateable<TestResponse>
    {
        public static TestResponse Create(Guid id) => new TestResponse(id);
    }

    public sealed record ErroringRequest(Guid Id, int N) : IRequest<TestResponse>;

    public sealed class ErroringRequestHandler : IRequestHandler<ErroringRequest, TestResponse>
    {
        public ValueTask<TestResponse> Handle(ErroringRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
