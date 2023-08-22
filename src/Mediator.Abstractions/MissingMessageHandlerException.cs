namespace Mediator;

/// <summary>
/// Exception that is thrown when Mediator receives messages
/// that have no registered handlers.
/// </summary>
public class MissingMessageHandlerException : Exception
{
    public object? MediatorMessage { get; }

    public MissingMessageHandlerException(object? message)
        : base("No handler registered for message type: " + message?.GetType().FullName ?? "Unknown")
    {
        MediatorMessage = message;
    }
}
