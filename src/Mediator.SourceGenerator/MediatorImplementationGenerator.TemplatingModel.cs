using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.SourceGenerator
{
    public sealed record MessageData(MessageType RequestType, MessageType? ResponseType, IEnumerable<Handler> Handlers)
    {
        public bool IsNotification => RequestType.iMessageType == "INotification";

        public Handler SingleHandler => Handlers.FirstOrDefault();

        public int HandlerCount => Handlers.Count();

        public HandlerInterface SingleHandlerInterface => Handlers
            .SelectMany(h => h.Interfaces.Where(hi => hi.RequestType == RequestType && hi.ResponseType == ResponseType))
            .First();
    }

    internal sealed partial class MediatorImplementationGenerator
    {
        private sealed class TemplatingModel
        {
            public readonly string MediatorNamespace;
            public readonly IEnumerable<Handler> Handlers;
            public readonly IEnumerable<HandlerType> HandlerTypes;
            public readonly IEnumerable<MessageData> RequestTypes;

            public TemplatingModel(string mediatorNamespace, IEnumerable<Handler> handlers, IEnumerable<HandlerType> handlerTypes)
            {
                MediatorNamespace = mediatorNamespace;
                Handlers = handlers;
                HandlerTypes = handlerTypes;
                RequestTypes = Handlers
                    .SelectMany(h => h.Interfaces.Select(i => (RequestType: i.RequestType, ResponseType: i.ResponseType)))
                    .Distinct()
                    .Select(r => (Types: r, Handlers: handlers.Where(h => h.Interfaces.Any(hi => hi.RequestType == r.RequestType && hi.ResponseType == r.ResponseType)).ToArray()))
                    .Select(r => new MessageData(r.Types.RequestType, r.Types.ResponseType, r.Handlers))
                    .ToArray();
            }
        }
    }
}
