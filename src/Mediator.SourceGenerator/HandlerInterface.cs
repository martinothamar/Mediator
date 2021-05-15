using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator
{
    public sealed class HandlerInterface : IEquatable<HandlerInterface?>
    {
        public readonly INamedTypeSymbol Symbol;
        public readonly string FullName;
        public readonly string MessageType;
        public readonly string ReturnType;
        public readonly string MethodName;
        public readonly MessageType RequestType;
        public readonly MessageType? ResponseType;

        private readonly List<Handler> _concreteHandlers = new();

        public IEnumerable<Handler> ConcreteHandlers => _concreteHandlers;

        public bool HasResponse => ResponseType is not null;

        public bool IsNotificationType => RequestType.iMessageType == "INotification";

        public HandlerInterface(INamedTypeSymbol baseHandlerSymbol, Compilation compilation)
        {
            Symbol = baseHandlerSymbol;

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
                "INotification" => ("Publish", "Publish"),
                _ => ("Send", "Send"),
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
                _ when baseHandlerSymbol.Name == "INotificationHandler" => "Publish",
                _ => "Send",
            };
        }

        internal void AddConcreteHandlers(IEnumerable<Handler> allConcreteHandlers, Compilation compilation)
        {
            foreach (var concreteHandler in allConcreteHandlers)
            {
                var conversion = compilation.ClassifyConversion(concreteHandler.Symbol, Symbol);

                if (conversion.IsImplicit)
                {
                    if (!_concreteHandlers.Contains(concreteHandler))
                        _concreteHandlers.Add(concreteHandler);
                }
            }
        }

        public override bool Equals(object? obj) => Equals(obj as HandlerInterface);

        public bool Equals(HandlerInterface? other) => other is not null && SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol);

        public override int GetHashCode() => 1179485718 + SymbolEqualityComparer.Default.GetHashCode(Symbol);

        public static bool operator ==(HandlerInterface? left, HandlerInterface? right) => EqualityComparer<HandlerInterface>.Default.Equals(left!, right!);

        public static bool operator !=(HandlerInterface? left, HandlerInterface? right) => !(left == right);

        public override string ToString() => $"{{ FullName={FullName}, RequestType={RequestType}, ResponseType={ResponseType} }}";
    }
}
