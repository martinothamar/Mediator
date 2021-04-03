using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mediator.SourceGenerator
{
    internal sealed class CompilationAnalyzer
    {
        private readonly Compilation _compilation;
        private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> _handlerMap;

        public IReadOnlyDictionary<INamedTypeSymbol, List<INamedTypeSymbol>> HandlerMap => _handlerMap;

        public IEnumerable<INamedTypeSymbol> HandlerTypes { get; private set; }

        public string MediatorNamespace { get; private set; } = Constants.MediatorLib;

        public CompilationAnalyzer(Compilation compilation)
        {
            _compilation = compilation;
#pragma warning disable RS1024 // Compare symbols correctly
            _handlerMap = new (SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            HandlerTypes = Array.Empty<INamedTypeSymbol>();
        }

        public void Analyze(CancellationToken cancellationToken)
        {
            var compilation = _compilation;

            var queue = new Queue<INamespaceSymbol>();

            queue.Enqueue(compilation.Assembly.GlobalNamespace);

            var attrs = compilation.Assembly.GetAttributes();

            var optionsAttr = attrs.SingleOrDefault(a => a.AttributeClass?.Name == "MediatorOptions");
            if (optionsAttr is not null)
            {
                TryParseOptions(optionsAttr, cancellationToken);
            }

            var handlerTypes = new INamedTypeSymbol[]
            {
                // Handlers
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequestHandler`1")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequestHandler`2")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommandHandler`1")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommandHandler`2")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IQueryHandler`2")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.INotificationHandler`1")!.OriginalDefinition,
            };
            HandlerTypes = handlerTypes;

            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                    continue;

                if (!assemblySymbol.Modules.Any(m => m.ReferencedAssemblies.Any(ra => ra.Name == Constants.MediatorLib)))
                    continue;

                queue.Enqueue(assemblySymbol.GlobalNamespace);
            }

            while (queue.Count > 0)
            {
                var nsSymbol = queue.Dequeue();

                foreach (var member in nsSymbol.GetMembers())
                {
                    if (member is INamespaceSymbol childNsSymbol)
                    {
                        queue.Enqueue(childNsSymbol);
                        continue;
                    }

                    var typeSymbol = (INamedTypeSymbol)member;
                    List<INamedTypeSymbol>? handlerSymbolList = null;

                    foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
                    {
                        if (interfaceSymbol.ContainingNamespace.Name != Constants.MediatorLib)
                            continue;

                        if (!handlerTypes.Contains(interfaceSymbol.OriginalDefinition, SymbolEqualityComparer.Default))
                            continue;

                        handlerSymbolList ??= _handlerMap[typeSymbol.OriginalDefinition] = new();

                        handlerSymbolList.Add(interfaceSymbol);
                    }
                }
            }
        }

        private void TryParseOptions(AttributeData optionsAttr, CancellationToken cancellationToken)
        {
            var compilation = _compilation;

            var syntaxReference = optionsAttr.ApplicationSyntaxReference;
            if (syntaxReference is not null)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxReference.SyntaxTree);

                var optionsAttrSyntax = optionsAttr.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;
                if (optionsAttrSyntax is not null && optionsAttrSyntax.ArgumentList is not null)
                {
                    var namespaceArg = semanticModel.GetConstantValue(optionsAttrSyntax.ArgumentList.Arguments[0].Expression, cancellationToken).Value as string;
                    if (!string.IsNullOrWhiteSpace(namespaceArg))
                        MediatorNamespace = namespaceArg!;
                }
            }
        }
    }
}
