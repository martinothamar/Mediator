using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.SourceGenerator
{
    public sealed record MessageData(MessageType RequestType, MessageType? ResponseType, IEnumerable<Handler> Handlers)
    {
        public bool IsNotification => RequestType.iMessageType == "INotification";
        public bool IsRequest => RequestType.iMessageType == "IRequest";
        public bool IsCommand => RequestType.iMessageType == "ICommand";
        public bool IsQuery => RequestType.iMessageType == "IQuery";

        public Handler SingleHandler => Handlers.FirstOrDefault();

        public int HandlerCount => Handlers.Count();

        public HandlerInterface SingleHandlerInterface => Handlers
            .SelectMany(h => h.Interfaces.Where(hi => hi.RequestType == RequestType && hi.ResponseType == ResponseType))
            .First();
    }
}
