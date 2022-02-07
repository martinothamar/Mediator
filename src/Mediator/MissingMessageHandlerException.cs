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

    public class MissingStreamMessageHandlerException : Exception
    {
        public IStreamMessage? MediatorMessage { get; }

        public MissingStreamMessageHandlerException(IStreamMessage? message)
            : base("No handler reqistered for message type: " + message?.GetType().FullName ?? "Unknown")
        {
            MediatorMessage = message;
        }
    }
}
