using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator
{
    internal sealed partial class MediatorImplementationGenerator
    {
        private sealed class TemplatingModel
        {
            public readonly string MediatorNamespace;
            public readonly IEnumerable<Handler> Handlers;
            public readonly IEnumerable<HandlerType> HandlerTypes;

            public TemplatingModel(string mediatorNamespace, IEnumerable<Handler> handlers, IEnumerable<HandlerType> handlerTypes)
            {
                MediatorNamespace = mediatorNamespace;
                Handlers = handlers;
                HandlerTypes = handlerTypes;
            }

            public sealed record HandlerType(string Name, bool HasResponse);

            public sealed class Handler
            {
                public readonly string FullName;

                public readonly IEnumerable<HandlerInterface> Interfaces;

                public Handler(INamedTypeSymbol handlerType, IEnumerable<INamedTypeSymbol> handlerInterfaces, Compilation compilation)
                {
                    FullName = GetTypeSymbolFullName(handlerType);

                    Interfaces = handlerInterfaces
                        .Select(handler => new HandlerInterface(handler, compilation))
                        .ToArray();
                }
            }

            public sealed class HandlerInterface
            {
                public readonly string FullName;
                public readonly string MessageType;
                public readonly string ReturnType;
                public readonly string MethodName;
                public readonly MessageType RequestType;
                public readonly MessageType? ResponseType;

                public bool HasResponse => ResponseType is not null;

                public HandlerInterface(INamedTypeSymbol handler, Compilation compilation)
                {
                    var requestTypeName = GetTypeSymbolFullName(handler.TypeArguments[0]);
                    string? responseTypeName = null;

                    ITypeSymbol? responseTypeSymbol = null;
                    if (handler.TypeArguments.Length > 1)
                    {
                        responseTypeSymbol = handler.TypeArguments[1];
                        responseTypeName = GetTypeSymbolFullName(responseTypeSymbol);
                    }

                    FullName = GetTypeSymbolFullName(handler);
                    MessageType = handler.OriginalDefinition.TypeArguments[0].Name.Substring(1);
                    RequestType = new MessageType(requestTypeName);
                    ResponseType = responseTypeName is null ? null : new MessageType(responseTypeName);

                    ReturnType = ResponseType is null ?
                        GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask).FullName!)!) :
                        GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask<>).FullName!)!.Construct(responseTypeSymbol!));

                    MethodName = handler switch
                    {
                        _ when handler.Name == "INotificationHandler" => "Publish",
                        _ => "Send",
                    };
                }
            }

            public sealed record MessageType(string FullName);

            static string GetTypeSymbolFullName(ITypeSymbol symbol)
            {
                return symbol.ToDisplayString(new SymbolDisplayFormat(
                    SymbolDisplayGlobalNamespaceStyle.Included,
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
                ));
            }
        }
    }
}
