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

    internal sealed partial class MediatorImplementationGenerator
    {
        private sealed class TemplatingModel
        {
            public readonly string MediatorNamespace;
            public readonly IEnumerable<Handler> Handlers;
            public readonly IEnumerable<HandlerInterface> InterfaceHandlers;
            public readonly IEnumerable<HandlerType> HandlerTypes;
            public readonly IEnumerable<MessageData> RequestTypes;

            public readonly bool HasRequestsWithoutReponse;
            public readonly bool HasRequestsWithReponse;
            public readonly bool HasCommandsWithoutReponse;
            public readonly bool HasCommandsWithReponse;
            public readonly bool HasQueriesWithResponse;

            public IEnumerable<MessageData> RequestWithoutResponseTypes => RequestTypes.Where(r => r.IsRequest && (r.ResponseType is null || r.ResponseType.IsUnitType));
            public IEnumerable<MessageData> RequestWithResponseTypes => RequestTypes.Where(r => r.IsRequest && r.ResponseType is not null && !r.ResponseType.IsUnitType);

            public IEnumerable<MessageData> CommandWithoutResponseTypes => RequestTypes.Where(r => r.IsCommand && (r.ResponseType is null || r.ResponseType.IsUnitType));
            public IEnumerable<MessageData> CommandWithResponseTypes => RequestTypes.Where(r => r.IsCommand && r.ResponseType is not null && !r.ResponseType.IsUnitType);

            public IEnumerable<MessageData> QueryWithResponseTypes => RequestTypes.Where(r => r.IsQuery && r.ResponseType is not null && !r.ResponseType.IsUnitType);

            public TemplatingModel(CompilationAnalyzer compilationAnalyzer)
            {
                MediatorNamespace = compilationAnalyzer.MediatorNamespace;
                Handlers = compilationAnalyzer.ConcreteHandlers;
                InterfaceHandlers = compilationAnalyzer.InterfaceHandlers;
                HandlerTypes = compilationAnalyzer.BaseHandlers;

                RequestTypes = Handlers
                    .SelectMany(h => h.Interfaces.Select(i => (RequestType: i.RequestType, ResponseType: i.ResponseType)))
                    .Distinct()
                    .Select(r => (Types: r, Handlers: Handlers.Where(h => h.Interfaces.Any(hi => hi.RequestType == r.RequestType && hi.ResponseType == r.ResponseType)).ToArray()))
                    .Select(r => new MessageData(r.Types.RequestType, r.Types.ResponseType, r.Handlers))
                    .ToArray();

                HasRequestsWithoutReponse = RequestTypes.Any(r => r.IsRequest && (r.ResponseType is null || r.ResponseType.IsUnitType));
                HasRequestsWithReponse = RequestTypes.Any(r => r.IsRequest && r.ResponseType is not null && !r.ResponseType.IsUnitType);

                HasCommandsWithoutReponse = RequestTypes.Any(r => r.IsCommand && (r.ResponseType is null || r.ResponseType.IsUnitType));
                HasCommandsWithReponse = RequestTypes.Any(r => r.IsCommand && r.ResponseType is not null && !r.ResponseType.IsUnitType);

                HasQueriesWithResponse = RequestTypes.Any(r => r.IsQuery && r.ResponseType is not null && !r.ResponseType.IsUnitType);
            }
        }
    }
}
