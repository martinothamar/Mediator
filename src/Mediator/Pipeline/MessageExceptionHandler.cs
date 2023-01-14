namespace Mediator;

public readonly struct ExceptionHandlingResult<TResponse>
{
    internal readonly TResponse _response;
    internal readonly bool _handled;

    public static readonly ExceptionHandlingResult<TResponse> NotHandled = new ExceptionHandlingResult<TResponse>(
        false,
        default!
    );

    public static ExceptionHandlingResult<TResponse> Handled(TResponse response) =>
        new ExceptionHandlingResult<TResponse>(true, response);

    private ExceptionHandlingResult(bool handled, TResponse response)
    {
        _handled = handled;
        _response = response;
    }

    public static implicit operator ValueTask<ExceptionHandlingResult<TResponse>>(
        ExceptionHandlingResult<TResponse> result
    ) => new ValueTask<ExceptionHandlingResult<TResponse>>(result);
}

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
            var result = await Handle(message, exception, cancellationToken);

            if (!result._handled)
                throw;

            return result._response;
        }
    }

    protected abstract ValueTask<ExceptionHandlingResult<TResponse>> Handle(
        TMessage message,
        TException exception,
        CancellationToken cancellationToken
    );

    protected static readonly ExceptionHandlingResult<TResponse> NotHandled =
        ExceptionHandlingResult<TResponse>.NotHandled;

    protected static ExceptionHandlingResult<TResponse> Handled(TResponse response) =>
        ExceptionHandlingResult<TResponse>.Handled(response);
}

public abstract class MessageExceptionHandler<TMessage, TResponse>
    : MessageExceptionHandler<TMessage, TResponse, Exception> where TMessage : notnull, IMessage { }
