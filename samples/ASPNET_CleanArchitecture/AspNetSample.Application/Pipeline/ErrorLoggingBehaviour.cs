using Mediator;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSample.Application
{
    public class ErrorLoggingBehaviour<TMessage> : IPipelineBehavior<TMessage>
        where TMessage : IMessage
    {
        protected readonly ILogger _logger;

        public ErrorLoggingBehaviour(ILogger<ErrorLoggingBehaviour<TMessage>> logger) => _logger = logger;
        protected ErrorLoggingBehaviour(ILogger logger) => _logger = logger;

        public async ValueTask Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage> next)
        {
            try
            {
                await next(message, cancellationToken);
            }
            catch (Exception ex)
            {
                LogError(ex, message);
                throw;
            }
        }

        protected void LogError(Exception ex, TMessage message)
        {
            _logger.LogError(ex, "Error handling message of type {messageType}", message.GetType().Name);
        }
    }

    public sealed class ErrorLoggingBehaviour<TMessage, TResponse> : ErrorLoggingBehaviour<TMessage>, IPipelineBehavior<TMessage, TResponse>
        where TMessage : IMessage
    {
        public ErrorLoggingBehaviour(ILogger<ErrorLoggingBehaviour<TMessage, TResponse>> logger)
            : base(logger)
        {
        }

        public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
        {
            try
            {
                return await next(message, cancellationToken);
            }
            catch (Exception ex)
            {
                LogError(ex, message);
                throw;
            }
        }
    }
}
