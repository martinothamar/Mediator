using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public HandlerInterface(INamedTypeSymbol baseHandlerSymbol, INamedTypeSymbol unitSymbol, Compilation compilation)
        {
            Symbol = baseHandlerSymbol;

            INamedTypeSymbol requestType = (INamedTypeSymbol)baseHandlerSymbol.TypeArguments[0];
            INamedTypeSymbol? responseTypeSymbol = null;

            if (baseHandlerSymbol.TypeArguments.Length > 1)
                responseTypeSymbol = (INamedTypeSymbol)baseHandlerSymbol.TypeArguments[1];

            var baseHandlerSymbolWithResponse = baseHandlerSymbol.OriginalDefinition.AllInterfaces.SingleOrDefault(i => i.ContainingNamespace?.Name == Constants.MediatorLib);
            if (baseHandlerSymbolWithResponse is not null)
                responseTypeSymbol = (INamedTypeSymbol)baseHandlerSymbolWithResponse.TypeArguments[1];

            FullName = RoslynExtensions.GetTypeSymbolFullName(baseHandlerSymbol);
            MessageType = baseHandlerSymbol.OriginalDefinition.TypeArguments[0].Name.Substring(1);

            RequestType = new MessageType(requestType, baseHandlerSymbol, unitSymbol, compilation);
            ResponseType = responseTypeSymbol is null ? null : new MessageType(responseTypeSymbol, baseHandlerSymbol, unitSymbol, compilation);

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

        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(Symbol);

        public static bool operator ==(HandlerInterface? left, HandlerInterface? right) => EqualityComparer<HandlerInterface>.Default.Equals(left!, right!);

        public static bool operator !=(HandlerInterface? left, HandlerInterface? right) => !(left == right);

        public override string ToString() => $"{{ FullName={FullName}, RequestType={RequestType}, ResponseType={ResponseType} }}";
    }
}
