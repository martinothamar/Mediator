using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator
{
    public sealed class MessageType : IEquatable<MessageType>
    {
        public INamedTypeSymbol Symbol { get; }
        public string FullName { get; }
        public string iMessageType { get; }
        public string SyncMethodName { get; }
        public string AsyncMethodName { get; }
        public string SyncReturnType { get; }
        public string AsyncReturnType { get; }

        public bool IsUnitType { get; }

        public bool IsStruct { get; }
        public bool IsReadOnly { get; }

        public MessageType(INamedTypeSymbol symbol, INamedTypeSymbol baseHandlerSymbol, INamedTypeSymbol unitSymbol, Compilation compilation)
        {
            Symbol = symbol;

            IsUnitType = SymbolEqualityComparer.Default.Equals(symbol, unitSymbol);

            FullName = RoslynExtensions.GetTypeSymbolFullName(symbol);
            string? responseTypeName = null;

            ITypeSymbol? responseTypeSymbol = null;
            if (baseHandlerSymbol.TypeArguments.Length > 1)
            {
                responseTypeSymbol = baseHandlerSymbol.TypeArguments[1];
                responseTypeName = RoslynExtensions.GetTypeSymbolFullName(responseTypeSymbol);
            }

            iMessageType = baseHandlerSymbol.Name.Substring(0, baseHandlerSymbol.Name.IndexOf("Handler"));
            (SyncMethodName, AsyncMethodName) = iMessageType switch
            {
                "INotification" => ("Publish", "Publish"),
                _ => ("Send", "Send"),
            };

            (SyncReturnType, AsyncReturnType) = responseTypeName is null ?
                ("void", RoslynExtensions.GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask).FullName!)!)) :
                (RoslynExtensions.GetTypeSymbolFullName(responseTypeSymbol!), RoslynExtensions.GetTypeSymbolFullName(compilation.GetTypeByMetadataName(typeof(ValueTask<>).FullName!)!.Construct(responseTypeSymbol!)));

            IsStruct = symbol.TypeKind == TypeKind.Struct;
            IsReadOnly = symbol.IsReadOnly;
        }


        public override bool Equals(object? obj) => Equals(obj as MessageType);

        public bool Equals(MessageType? other) => other is not null && SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol);

        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(Symbol);

        public static bool operator ==(MessageType? left, MessageType? right) => EqualityComparer<MessageType>.Default.Equals(left!, right!);

        public static bool operator !=(MessageType? left, MessageType? right) => !(left == right);

        public override string ToString() => $"{{ FullName={FullName}, IMessageType={iMessageType} }}";
    }
}
