using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.SourceGenerator
{
    public sealed class Handler
    {
        public readonly string FullName;

        public readonly IEnumerable<HandlerInterface> Interfaces;

        public Handler(INamedTypeSymbol handlerType, IEnumerable<INamedTypeSymbol> handlerInterfaces, Compilation compilation)
        {
            FullName = RoslynExtensions.GetTypeSymbolFullName(handlerType);

            Interfaces = handlerInterfaces
                .Select(handler => new HandlerInterface(handlerType, handler, compilation))
                .ToArray();
        }
    }
}
