using Mediator.Tests.TestTypes;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline
{
    public sealed class SomePipeline : IPipelineBehavior<SomeRequest, SomeResponse>, IPipelineTestData
    {
        public Guid Id { get; private set; }
        public long LastMsgTimestamp { get; private set; }

        public ValueTask<SomeResponse> Handle(SomeRequest message, CancellationToken cancellationToken, MessageHandlerDelegate<SomeRequest, SomeResponse> next)
        {
            LastMsgTimestamp = Stopwatch.GetTimestamp();

            if (message is null || message.Id == default)
                throw new ArgumentException("Invalid input");

            Id = message.Id;

            return next(message, cancellationToken);
        }
    }

    public sealed class SomeGenericConstrainedPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest // Only requests, not commands or queries
    {
        public async ValueTask<TResponse> Handle(TRequest message, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, TResponse> next)
        {
            var response = await next(message, cancellationToken);
            if (response is SomeResponse someResponse)
                return (TResponse)(object)(someResponse with { Id = Guid.NewGuid() });
            else
                return response;
        }
    }
}
