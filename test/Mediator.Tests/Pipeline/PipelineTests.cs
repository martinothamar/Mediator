using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests.Pipeline
{
    public interface IPipelineTestData
    {
        Guid Id { get; }

        public long LastMsgTimestamp { get; }
    }

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

    public sealed class GenericPipeline<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>, IPipelineTestData
        where TMessage : IMessage
    {
        internal TMessage? Message;
        public Guid Id => (Guid)Message!.GetType()!.GetProperty("Id")!.GetValue(Message)!;
        public long LastMsgTimestamp { get; private set; }

        public ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
        {
            LastMsgTimestamp = Stopwatch.GetTimestamp();

            Message = message;

            return next(message, cancellationToken);
        }
    }

    public sealed class PipelineTests
    {
        [Fact]
        public async Task Test_Pipeline()
        {
            var (sp, mediator) = Fixture.GetMediator(services =>
            {
                services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>();
            });

            var id = Guid.NewGuid();

            var pipelineStep = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is SomePipeline) as SomePipeline;
            Assert.NotNull(pipelineStep);
            var response = await mediator.Send(new SomeRequest(id));
            Assert.Equal(id, response.Id);
            Assert.Equal(id, pipelineStep!.Id);
        }

        [Fact]
        public async Task Test_Generic_Pipeline()
        {
            var (sp, mediator) = Fixture.GetMediator(services =>
            {
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
            });

            var id = Guid.NewGuid();

            var pipelineStep = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is GenericPipeline<SomeRequest, SomeResponse>) as GenericPipeline<SomeRequest, SomeResponse>;
            Assert.NotNull(pipelineStep);
            var response = await mediator.Send(new SomeRequest(id));
            Assert.Equal(id, response.Id);
            Assert.Equal(id, pipelineStep!.Id);
        }

        [Fact]
        public async Task Test_Pipeline_Ordering()
        {
            var (sp, mediator) = Fixture.GetMediator(services =>
            {
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
                services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>();
            });

            var id = Guid.NewGuid();

            var response = await mediator.Send(new SomeRequest(id));

            var pipelineSteps = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Cast<IPipelineTestData>();

            var original = pipelineSteps.Select(p => p.LastMsgTimestamp).ToArray();
            var ordered = pipelineSteps.Select(p => p.LastMsgTimestamp).OrderBy(x => x).ToArray();
            Assert.True(original.SequenceEqual(ordered));
        }
    }
}
