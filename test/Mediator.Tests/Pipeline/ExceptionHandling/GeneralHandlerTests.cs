using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline.ExceptionHandling;

public class GeneralHandlerTests
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

        var response = await mediator.Send(new ErroringRequest(id));
        Assert.Equal(id, response.Id);
        Assert.Equal(typeof(NotImplementedException), state?.Exception?.GetType());
    }

    private sealed class State
    {
        public Exception? Exception { get; set; }
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

    public sealed record ErroringRequest(Guid Id) : IRequest<TestResponse>;

    public sealed class ErroringRequestHandler : IRequestHandler<ErroringRequest, TestResponse>
    {
        public ValueTask<TestResponse> Handle(ErroringRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
