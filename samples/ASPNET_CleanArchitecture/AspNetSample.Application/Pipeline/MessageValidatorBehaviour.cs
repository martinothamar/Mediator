using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSample.Application
{
    public sealed class MessageValidatorBehaviour<TMessage> : IPipelineBehavior<TMessage>
        where TMessage : IValidate
    {
        public ValueTask Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage> next)
        {
            if (!message.IsValid(out var validationError))
                throw new ValidationException(validationError);

            return next(message, cancellationToken);
        }
    }

    public sealed class MessageValidatorBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
        where TMessage : IValidate
    {
        public ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
        {
            if (!message.IsValid(out var validationError))
                throw new ValidationException(validationError);

            return next(message, cancellationToken);
        }
    }
}
