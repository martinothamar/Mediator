using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.SourceGenerator
{
    public sealed class Handler : IEquatable<Handler?>
    {
        public readonly INamedTypeSymbol Symbol;
        public readonly string FullName;

        public readonly IEnumerable<HandlerInterface> Interfaces;

        public Handler(INamedTypeSymbol handlerType, IEnumerable<INamedTypeSymbol> handlerInterfaces, INamedTypeSymbol unitSymbol, Compilation compilation)
        {
            Symbol = handlerType;
            FullName = RoslynExtensions.GetTypeSymbolFullName(handlerType);

            Interfaces = handlerInterfaces
                .Select(handler => new HandlerInterface(handler, unitSymbol, compilation))
                .ToArray();
        }

        public override bool Equals(object? obj) => Equals(obj as Handler);

        public bool Equals(Handler? other) => other is not null && SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol);

        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(Symbol);

        public static bool operator ==(Handler? left, Handler? right) => EqualityComparer<Handler>.Default.Equals(left!, right!);

        public static bool operator !=(Handler? left, Handler? right) => !(left == right);

        public override string ToString() => $"{{ FullName={FullName} }}";
    }
}
