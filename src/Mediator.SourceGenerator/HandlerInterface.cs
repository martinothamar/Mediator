using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator
{
    public sealed class HandlerInterface
    {
        public readonly string FullName;
        public readonly string MessageType;
        public readonly string ReturnType;
        public readonly string MethodName;
        public readonly MessageType RequestType;
        public readonly MessageType? ResponseType;

        public bool HasResponse => ResponseType is not null;

        public bool IsNotificationType => RequestType.iMessageType == "INotification";

        public HandlerInterface(INamedTypeSymbol concreteHandlerSymbol, INamedTypeSymbol baseHandlerSymbol, Compilation compilation)
        {
            var requestType = baseHandlerSymbol.TypeArguments[0];
            var requestTypeName = RoslynExtensions.GetTypeSymbolFullName(requestType);
            string? responseTypeName = null;

            ITypeSymbol? responseTypeSymbol = null;
            if (baseHandlerSymbol.TypeArguments.Length > 1)
            {
                responseTypeSymbol = baseHandlerSymbol.TypeArguments[1];
                responseTypeName = RoslynExtensions.GetTypeSymbolFullName(responseTypeSymbol);
            }

            FullName = RoslynExtensions.GetTypeSymbolFullName(baseHandlerSymbol);
            MessageType = baseHandlerSymbol.OriginalDefinition.TypeArguments[0].Name.Substring(1);

            var iMessageType = baseHandlerSymbol.Name.Substring(0, baseHandlerSymbol.Name.IndexOf("Handler"));
            var (syncMethodName, asyncMethodName) = iMessageType switch
            {
                "INotification" => ("Publish", "PublishAsync"),
                _ => ("Send", "SendAsync"),
            };

            var (syncReturnType, asyncReturnType) = responseTypeName is null ?
                ("void", RoslynExtensions.GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask).FullName!)!)) :
                (RoslynExtensions.GetTypeSymbolFullName(responseTypeSymbol!), RoslynExtensions.GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask<>).FullName!)!.Construct(responseTypeSymbol!)));

            RequestType = new MessageType(requestTypeName, iMessageType, syncMethodName, asyncMethodName, syncReturnType, asyncReturnType);
            ResponseType = responseTypeName is null ? null : new MessageType(responseTypeName, iMessageType, syncMethodName, asyncMethodName, syncReturnType, asyncReturnType);

            ReturnType = ResponseType is null ?
                RoslynExtensions.GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask).FullName!)!) :
                RoslynExtensions.GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask<>).FullName!)!.Construct(responseTypeSymbol!));

            MethodName = baseHandlerSymbol switch
            {
                _ when baseHandlerSymbol.Name == "INotificationHandler" => "PublishAsync",
                _ => "SendAsync",
            };
        }
    }
}
