namespace Mediator;

public class MissingMessageHandlerException : Exception
{
    public object? MediatorMessage { get; }

    public MissingMessageHandlerException(object? message)
        : base("No handler reqistered for message type: " + message?.GetType().FullName ?? "Unknown")
    {
        MediatorMessage = message;
    }
}
