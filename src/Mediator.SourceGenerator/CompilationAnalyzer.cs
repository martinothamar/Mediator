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
        private static readonly SymbolEqualityComparer _symbolEquality = SymbolEqualityComparer.Default;

        private readonly Compilation _compilation;
        private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> _concreteHandlerSymbolMap;

        public IReadOnlyDictionary<INamedTypeSymbol, List<INamedTypeSymbol>> ConcreteHandlerSymbolMap => _concreteHandlerSymbolMap;

        public IEnumerable<INamedTypeSymbol> BaseHandlerSymbols { get; private set; }

        public IEnumerable<INamedTypeSymbol> ConcreteMessageSymbols { get; private set; }

        public IEnumerable<HandlerType> BaseHandlers { get; private set; }

        public IEnumerable<Handler> ConcreteHandlers { get; private set; }

        public IEnumerable<HandlerInterface> InterfaceHandlers { get; private set; }

        public INamedTypeSymbol UnitSymbol { get; private set; }

        public string MediatorNamespace { get; private set; } = Constants.MediatorLib;

        public CompilationAnalyzer(Compilation compilation)
        {
            _compilation = compilation;
#pragma warning disable RS1024 // Compare symbols correctly
            _concreteHandlerSymbolMap = new (_symbolEquality);
#pragma warning restore RS1024 // Compare symbols correctly
            BaseHandlerSymbols = Array.Empty<INamedTypeSymbol>();
            ConcreteMessageSymbols = Array.Empty<INamedTypeSymbol>();

            BaseHandlers = Array.Empty<HandlerType>();
            ConcreteHandlers = Array.Empty<Handler>();
            InterfaceHandlers = Array.Empty<HandlerInterface>();

            UnitSymbol = null!;
        }

        public void Analyze(CancellationToken cancellationToken)
        {
            var compilation = _compilation;

            var queue = new Queue<INamespaceOrTypeSymbol>();

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

            UnitSymbol = compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.Unit")!.OriginalDefinition;

            var concreteMessageSymbols = new HashSet<INamedTypeSymbol>(_symbolEquality);

            FindGlobalNamespaces(queue);

            PopulateMetadata(queue, baseHandlerSymbols, baseMessageSymbols, concreteMessageSymbols);

            AddOpenGenerics(concreteMessageSymbols);

            CleanupGenericHandlers();

            UpdateState(concreteMessageSymbols);
        }

        private void UpdateState(HashSet<INamedTypeSymbol> concreteMessageSymbols)
        {
            ConcreteHandlers = _concreteHandlerSymbolMap
                .OrderBy(h => h.Key.Name)
                .Select(h => new Handler(h.Key, h.Value, UnitSymbol, _compilation))
                .ToArray();

            InterfaceHandlers = ConcreteHandlers
                .SelectMany(h => h.Interfaces)
                .Distinct()
                .ToArray();

            ConcreteMessageSymbols = concreteMessageSymbols.ToArray();

            AddPolymorphicDispatch();

            BaseHandlers = BaseHandlerSymbols
                .Select(ht =>
                {
                    var messageType = ht.TypeArguments[0].Name.Substring(1);
                    var hasResponse = ht.MetadataName.EndsWith("2", StringComparison.InvariantCulture);
                    return new HandlerType(messageType, hasResponse);
                })
                .ToArray();
        }

        private void AddPolymorphicDispatch()
        {
            foreach (var notification in ConcreteMessageSymbols.Where(m => !InterfaceHandlers.Any(h => _symbolEquality.Equals(h.RequestType.Symbol, m))))
            {
                foreach (var handlerInterface in InterfaceHandlers)
                {
                    var conversion = _compilation.ClassifyConversion(notification, handlerInterface.RequestType.Symbol);

                    if (conversion.IsImplicit)
                    {
                    }
                }
            }

            foreach (var handlerInterface in InterfaceHandlers)
            {
                handlerInterface.AddConcreteHandlers(ConcreteHandlers, _compilation);
            }
        }

        private void CleanupGenericHandlers()
        {
            var toDelete = new List<INamedTypeSymbol>();
            foreach (var kvp in _concreteHandlerSymbolMap)
            {
                if (IsOpenGeneric(kvp.Key))
                    toDelete.Add(kvp.Key);
            }

            toDelete.ForEach(s => _concreteHandlerSymbolMap.Remove(s));
        }

        private void AddOpenGenerics(HashSet<INamedTypeSymbol> concreteMessageSymbols)
        {
            var concreteHandlerSymbolMap = _concreteHandlerSymbolMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var symbol in concreteHandlerSymbolMap.SelectMany(kvp => kvp.Value).Distinct(_symbolEquality))
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
                                if (!_symbolEquality.Equals(concreteMessageSymbol, constraint) && _compilation.ClassifyConversion(concreteMessageSymbol, constraint).IsImplicit)
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
                            if (!_symbolEquality.Equals(concreteMessageSymbol, constraint) && _compilation.ClassifyConversion(concreteMessageSymbol, constraint).IsImplicit)
                            {
                                var constructedConcreteHandler = concreteHandler.Construct(concreteMessageSymbol);
                                AddInterfaceHandler(constructedConcreteHandler, handlerInterface);
                            }
                        }
                    }
                }
            }

        }
        static bool IsOpenGeneric(INamedTypeSymbol symbol) =>
            symbol.TypeArguments.Length > 0 && symbol.TypeArguments[0] is ITypeParameterSymbol;

        private void PopulateMetadata(
            Queue<INamespaceOrTypeSymbol> queue,
            INamedTypeSymbol[] baseHandlerSymbols,
            INamedTypeSymbol[] baseMessageSymbols,
            HashSet<INamedTypeSymbol> concreteMessageSymbols
        )
        {
            var context = (
                queue,
                baseHandlerSymbols,
                baseMessageSymbols,
                concreteMessageSymbols
            );

            while (queue.Count > 0)
            {
                var nsOrTypeSymbol = queue.Dequeue();

                if (nsOrTypeSymbol is INamespaceSymbol nsSymbol)
                {
                    foreach (var member in nsSymbol.GetMembers())
                    {
                        ProcessMember(member, in context);
                    }
                }
                else
                {
                    var typeSymbol = (INamedTypeSymbol)nsOrTypeSymbol;

                    ProcessMember(typeSymbol, in context);
                }

                void ProcessMember(
                    INamespaceOrTypeSymbol member,
                    in (
                        Queue<INamespaceOrTypeSymbol> queue,
                        INamedTypeSymbol[] baseHandlerSymbols,
                        INamedTypeSymbol[] baseMessageSymbols,
                        HashSet<INamedTypeSymbol> concreteMessageSymbols
                    ) context
                )
                {
                    if (member is INamespaceSymbol childNsSymbol)
                    {
                        context.queue.Enqueue(childNsSymbol);
                        return;
                    }

                    var typeSymbol = (INamedTypeSymbol)member;

                    foreach (var childTypeSymbol in typeSymbol.GetTypeMembers())
                        context.queue.Enqueue(childTypeSymbol);

                    if (typeSymbol.IsStatic)
                        return;

                    List<INamedTypeSymbol>? handlerSymbolList = null;

                    foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
                    {
                        if (interfaceSymbol.ContainingNamespace.Name != Constants.MediatorLib)
                            continue;

                        if (context.baseHandlerSymbols.Contains(interfaceSymbol.OriginalDefinition, _symbolEquality))
                        {
                            if (handlerSymbolList is null)
                            {
                                handlerSymbolList = new();
                                _concreteHandlerSymbolMap.Add(typeSymbol.OriginalDefinition, handlerSymbolList);
                            }

                            var shouldSkip = interfaceSymbol.TypeArguments.Length > 1 &&
                                interfaceSymbol.TypeArguments[1] is INamedTypeSymbol responseTypeSymbol &&
                                _symbolEquality.Equals(responseTypeSymbol, UnitSymbol) &&
                                handlerSymbolList.Any(h => h.AllInterfaces.Any(i => _symbolEquality.Equals(i, interfaceSymbol)));

                            if (shouldSkip)
                                continue;

                            handlerSymbolList.Add(interfaceSymbol);
                        }
                        else if (context.baseMessageSymbols.Contains(interfaceSymbol.OriginalDefinition, _symbolEquality))
                        {
                            context.concreteMessageSymbols.Add(typeSymbol);
                        }
                    }
                }
            }
        }

        private void FindGlobalNamespaces(Queue<INamespaceOrTypeSymbol> queue)
        {
            queue.Enqueue(_compilation.Assembly.GlobalNamespace);

            foreach (var reference in _compilation.References)
            {
                if (_compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                    continue;

                if (!assemblySymbol.Modules.Any(m => m.ReferencedAssemblies.Any(ra => ra.Name == Constants.MediatorLib)))
                    continue;

                queue.Enqueue(assemblySymbol.GlobalNamespace);
            }
        }

        private void AddInterfaceHandler(INamedTypeSymbol concrete, INamedTypeSymbol @interface)
        {
            if (!_concreteHandlerSymbolMap.TryGetValue(concrete, out var list))
                _concreteHandlerSymbolMap[concrete] = list = new();

            if (!list.Contains(@interface, _symbolEquality))
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
