namespace Mediator;

/// <summary>
/// Exception thrown when Mediator receives messages that don't derive from the correct interfaces
/// from Mediator.Abstractions.
/// </summary>
public class InvalidMessageException : Exception
{
    public object? MediatorMessage { get; }

    public InvalidMessageException(object? message)
        : base("Tried to send/publish invalid message type to Mediator: " + message?.GetType().FullName ?? "Unknown")
    {
        MediatorMessage = message;
    }
}
