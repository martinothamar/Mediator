using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline.ExceptionHandling;

public class ConcreteExceptionHandlerTests
{
    [Fact]
    public async Task Test_ExceptionHandler()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<State>();
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionHandler<,>));
            }
        );

        var id = Guid.NewGuid();

        var state = sp.GetRequiredService<State>();
        Assert.NotNull(state);

        var response = await mediator.Send(new ErroringRequest(id, 0));
        Assert.Equal(id, response.Id);
        Assert.Equal(typeof(NotImplementedException), state?.Exception?.GetType());

        await Assert.ThrowsAsync<ArgumentException>(async () => await mediator.Send(new ErroringRequest(id, 1)));
    }

    public sealed class State
    {
        public Exception? Exception { get; set; }
    }

    private sealed class ExceptionHandler<TMessage, TResponse>
        : MessageExceptionHandler<TMessage, TResponse, NotImplementedException>
        where TMessage : notnull, IMessage
        where TResponse : ICreateable<TResponse>
    {
        private readonly State _state;

        public ExceptionHandler(State state) => _state = state;

        protected override ValueTask<ExceptionHandlingResult<TResponse>> Handle(
            TMessage message,
            NotImplementedException exception,
            CancellationToken cancellationToken
        )
        {
            _state.Exception = exception;
            var id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            return Handled(TResponse.Create(id));
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
            if (request.N == 0)
                throw new NotImplementedException();
            else
                throw new ArgumentException();
        }
    }
}
