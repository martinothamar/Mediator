using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> _concreteHandlerSymbolMap;

        public IReadOnlyDictionary<INamedTypeSymbol, List<INamedTypeSymbol>> ConcreteHandlerSymbolMap => _concreteHandlerSymbolMap;

        public IEnumerable<INamedTypeSymbol> BaseHandlerSymbols { get; private set; }

        public IEnumerable<INamedTypeSymbol> ConcreteMessageSymbols { get; private set; }

        public IEnumerable<HandlerType> BaseHandlers { get; private set; }

        public IEnumerable<Handler> ConcreteHandlers { get; private set; }

        public IEnumerable<HandlerInterface> InterfaceHandlers { get; private set; }

        public string MediatorNamespace { get; private set; } = Constants.MediatorLib;

        public CompilationAnalyzer(Compilation compilation)
        {
            _compilation = compilation;
#pragma warning disable RS1024 // Compare symbols correctly
            _concreteHandlerSymbolMap = new (SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            BaseHandlerSymbols = Array.Empty<INamedTypeSymbol>();
            ConcreteMessageSymbols = Array.Empty<INamedTypeSymbol>();

            BaseHandlers = Array.Empty<HandlerType>();
            ConcreteHandlers = Array.Empty<Handler>();
            InterfaceHandlers = Array.Empty<HandlerInterface>();
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

            var baseHandlerSymbols = new INamedTypeSymbol[]
            {
                // Handlers
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequestHandler`1")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequestHandler`2")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommandHandler`1")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommandHandler`2")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IQueryHandler`2")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.INotificationHandler`1")!.OriginalDefinition,
            };
            BaseHandlerSymbols = baseHandlerSymbols;

            var baseMessageSymbols = new INamedTypeSymbol[]
            {
                // Message types
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequest")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequest`1")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommand")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommand`1")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IQuery`1")!.OriginalDefinition,
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.INotification")!.OriginalDefinition,
            };

            var concreteMessageSymbols = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

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

                        if (baseHandlerSymbols.Contains(interfaceSymbol.OriginalDefinition, SymbolEqualityComparer.Default))
                        {
                            if (handlerSymbolList is null)
                            {
                                handlerSymbolList = new();
                                _concreteHandlerSymbolMap.Add(typeSymbol.OriginalDefinition, handlerSymbolList);
                            }

                            handlerSymbolList.Add(interfaceSymbol);
                        }
                        else if (baseMessageSymbols.Contains(interfaceSymbol.OriginalDefinition, SymbolEqualityComparer.Default))
                        {
                            concreteMessageSymbols.Add(typeSymbol);
                        }
                    }
                }
            }

            var concreteHandlerSymbolMap = _concreteHandlerSymbolMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var symbol in concreteHandlerSymbolMap.SelectMany(kvp => kvp.Value).Distinct(SymbolEqualityComparer.Default))
            {
                if (symbol is not INamedTypeSymbol handlerInterface)
                    continue;

                var isInterfaceOpenGeneric = IsOpenGeneric(handlerInterface);

                foreach (var concreteHandler in concreteHandlerSymbolMap.Keys)
                {
                    if (IsOpenGeneric(concreteHandler))
                    {
                        var typeParam = (ITypeParameterSymbol)concreteHandler.TypeArguments[0];

                        if (typeParam.ConstraintTypes.Length == 0)
                            throw new Exception("TODO report diag - unconstrained generic handler type");

                        var constraint = typeParam.ConstraintTypes[0];

                        if (isInterfaceOpenGeneric)
                        {
                            foreach (var concreteMessageSymbol in concreteMessageSymbols)
                            {
                                if (!SymbolEqualityComparer.Default.Equals(concreteMessageSymbol, constraint) && compilation.ClassifyConversion(concreteMessageSymbol, constraint).IsImplicit)
                                {
                                    var constructedConcreteHandler = concreteHandler.Construct(concreteMessageSymbol);
                                    var constructedInterfaceHandler = handlerInterface.OriginalDefinition.Construct(concreteMessageSymbol);
                                    AddInterfaceHandler(constructedConcreteHandler, constructedInterfaceHandler);
                                }
                            }
                        }
                        else
                        {
                            var concreteMessageSymbol = handlerInterface.TypeArguments[0];
                            if (!SymbolEqualityComparer.Default.Equals(concreteMessageSymbol, constraint) && compilation.ClassifyConversion(concreteMessageSymbol, constraint).IsImplicit)
                            {
                                var constructedConcreteHandler = concreteHandler.Construct(concreteMessageSymbol);
                                AddInterfaceHandler(constructedConcreteHandler, handlerInterface);
                            }
                        }
                    }
                }
            }

            var toDelete = new List<INamedTypeSymbol>();
            foreach (var kvp in _concreteHandlerSymbolMap)
            {
                if (IsOpenGeneric(kvp.Key))
                    toDelete.Add(kvp.Key);
            }

            toDelete.ForEach(s => _concreteHandlerSymbolMap.Remove(s));

            ConcreteHandlers = _concreteHandlerSymbolMap
                .OrderBy(h => h.Key.Name)
                .Select(h => new Handler(h.Key, h.Value, compilation))
                .ToArray();

            InterfaceHandlers = ConcreteHandlers
                .SelectMany(h => h.Interfaces)
                .Distinct()
                .Select(i =>
                {
                    i.AddConcreteHandlers(ConcreteHandlers, compilation);
                    return i;
                })
                .ToArray();

            ConcreteMessageSymbols = _concreteHandlerSymbolMap
                .SelectMany(h => h.Value.Select(s => s.TypeArguments[0]))
                .Where(s => s is INamedTypeSymbol)
                .Distinct(SymbolEqualityComparer.Default)
                .Cast<INamedTypeSymbol>()
                .ToArray();

            BaseHandlers = BaseHandlerSymbols
                .Select(ht =>
                {
                    var messageType = ht.TypeArguments[0].Name.Substring(1);
                    var hasResponse = ht.MetadataName.EndsWith("2", StringComparison.InvariantCulture);
                    return new HandlerType(messageType, hasResponse);
                })
                .ToArray();


            static bool IsOpenGeneric(INamedTypeSymbol symbol) => symbol.TypeArguments.Length > 0 && symbol.TypeArguments[0] is ITypeParameterSymbol;
        }

        private void AddInterfaceHandler(INamedTypeSymbol concrete, INamedTypeSymbol @interface)
        {
            if (!_concreteHandlerSymbolMap.TryGetValue(concrete, out var list))
                _concreteHandlerSymbolMap[concrete] = list = new();

            if (!list.Contains(@interface, SymbolEqualityComparer.Default))
                list.Add(@interface);
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
