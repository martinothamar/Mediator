using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline
{
    public sealed class GenericPipelineState : IPipelineTestData
    {
        internal object? Message;
        public Guid Id => Message is null ? default : (Guid)Message.GetType()!.GetProperty("Id")!.GetValue(Message)!;
        public long LastMsgTimestamp { get; private set; }

        public ValueTask<TResponse> Handle<TMessage, TResponse>(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
            where TMessage : IMessage
        {
            LastMsgTimestamp = Stopwatch.GetTimestamp();

            Message = message;

            return next(message, cancellationToken);
        }
    }

    public sealed class GenericPipeline<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>, IPipelineTestData
        where TMessage : IMessage
    {
        private readonly GenericPipelineState _state;

        public GenericPipeline(GenericPipelineState state) => _state = state;

        public Guid Id => _state.Id;

        public long LastMsgTimestamp => _state.LastMsgTimestamp;

        public ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next) =>
            _state.Handle(message, cancellationToken, next);
    }
}
