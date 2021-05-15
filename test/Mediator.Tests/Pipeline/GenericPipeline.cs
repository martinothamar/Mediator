using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline
{
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
}
