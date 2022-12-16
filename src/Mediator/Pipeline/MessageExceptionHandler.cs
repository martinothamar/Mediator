namespace Mediator;

public abstract class MessageExceptionHandler<TMessage, TResponse, TException> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TException : Exception
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next
    )
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (TException exception)
        {
            var (handled, response) = await Handle(message, exception, cancellationToken);

            if (!handled)
                throw;

            return response;
        }
    }

    protected abstract ValueTask<(bool Handled, TResponse Response)> Handle(
        TMessage message,
        TException exception,
        CancellationToken cancellationToken
    );
}

public abstract class MessageExceptionHandler<TMessage, TResponse>
    : MessageExceptionHandler<TMessage, TResponse, Exception> where TMessage : notnull, IMessage { }
