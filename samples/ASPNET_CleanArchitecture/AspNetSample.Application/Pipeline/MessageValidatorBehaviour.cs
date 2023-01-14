using Mediator;

namespace AspNetSample.Application;

public sealed class MessageValidatorBehaviour2<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
    where TMessage : IValidate
{
    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        if (!message.IsValid(out var validationError))
            throw new ValidationException(validationError);

        return default;
    }
}

public sealed class MessageValidatorBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IValidate
{
    public ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next
    )
    {
        if (!message.IsValid(out var validationError))
            throw new ValidationException(validationError);

        return next(message, cancellationToken);
    }
}
