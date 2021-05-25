using System;

namespace Mediator
{
    public class MissingMessageHandlerException : Exception
    {
        public IMessage? MediatorMessage { get; }

        public MissingMessageHandlerException(IMessage? message)
            : base("No handler reqistered for message type: " + message?.GetType().FullName ?? "Unknown")
        {
            MediatorMessage = message;
        }
    }
}
