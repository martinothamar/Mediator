using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Mediator.SourceGenerator;

internal readonly record struct CompilationAnalyzerContext(
    Compilation Compilation,
    IReadOnlyList<InvocationExpressionSyntax>? AddMediatorCalls,
    string GeneratorVersion,
    Action<Diagnostic> ReportDiagnostic,
    CancellationToken CancellationToken
);

internal sealed class CompilationAnalyzer
{
    private static readonly SymbolEqualityComparer _symbolComparer = SymbolEqualityComparer.Default;
    private readonly CompilationAnalyzerContext _context;

    public CompilationAnalyzerContext Context => _context;

    private readonly HashSet<RequestMessage> _requestMessages;
    private readonly HashSet<NotificationMessage> _notificationMessages;
    private readonly HashSet<RequestMessageHandler> _requestMessageHandlers;
    private readonly HashSet<NotificationMessageHandler> _notificationMessageHandlers;

    public ImmutableArray<RequestMessageHandlerWrapperModel> RequestMessageHandlerWrappers;

    private INamedTypeSymbol[] _baseHandlerSymbols;
    private INamedTypeSymbol[] _baseMessageSymbols;

    private INamedTypeSymbol? _notificationHandlerInterfaceSymbol;
    private INamedTypeSymbol? _notificationInterfaceSymbol;

    private bool HasErrors { get; set; }

    private INamedTypeSymbol? UnitSymbol;

    public Compilation Compilation => _context.Compilation;

    public string MediatorNamespace { get; private set; } = Constants.MediatorLib;

    private string GeneratorVersion =>
        string.IsNullOrWhiteSpace(_context.GeneratorVersion) ? "1.0.0.0" : _context.GeneratorVersion;

    private IFieldSymbol? _configuredLifetimeSymbol;
    private INamedTypeSymbol? _serviceLifetimeEnumSymbol;
    private IFieldSymbol? ServiceLifetimeSymbol => _configuredLifetimeSymbol ?? SingletonServiceLifetimeSymbol;
    private IFieldSymbol? SingletonServiceLifetimeSymbol;

    public string? ServiceLifetime => ServiceLifetimeSymbol?.GetFieldSymbolFullName();
    private string? ServiceLifetimeShort => ServiceLifetimeSymbol?.Name;

    private string? SingletonServiceLifetime => SingletonServiceLifetimeSymbol?.GetFieldSymbolFullName();

    public bool ServiceLifetimeIsSingleton => ServiceLifetimeSymbol?.Name == "Singleton";

    public bool ServiceLifetimeIsScoped => ServiceLifetimeSymbol?.Name == "Scoped";

    public bool ServiceLifetimeIsTransient => ServiceLifetimeSymbol?.Name == "Transient";

    private bool IsTestRun =>
        (_context.Compilation.AssemblyName?.StartsWith("Mediator.Tests") ?? false)
        || (_context.Compilation.AssemblyName?.StartsWith("Mediator.SmokeTest") ?? false);

    private bool ConfiguredViaAttribute { get; set; }

    private bool ConfiguredViaConfiguration { get; set; }

    public CompilationAnalyzer(in CompilationAnalyzerContext context)
    {
        _context = context;
        _requestMessages = new();
        _requestMessageHandlers = new();
        _notificationMessages = new();
        _notificationMessageHandlers = new();
        _baseHandlerSymbols = Array.Empty<INamedTypeSymbol>();
        _baseMessageSymbols = Array.Empty<INamedTypeSymbol>();
    }

    public void Initialize()
    {
        try
        {
            TryLoadUnitSymbol(out UnitSymbol);

            TryLoadBaseHandlerSymbols(out _baseHandlerSymbols);

            TryLoadDISymbols(out _serviceLifetimeEnumSymbol, out SingletonServiceLifetimeSymbol);

            TryParseConfiguration();

            RequestMessageHandlerWrappers = new RequestMessageHandlerWrapperModel[]
            {
                new RequestMessageHandlerWrapperModel("Request", this),
                new RequestMessageHandlerWrapperModel("StreamRequest", this),
                new RequestMessageHandlerWrapperModel("Command", this),
                new RequestMessageHandlerWrapperModel("StreamCommand", this),
                new RequestMessageHandlerWrapperModel("Query", this),
                new RequestMessageHandlerWrapperModel("StreamQuery", this),
            }.ToImmutableArray();

            _notificationHandlerInterfaceSymbol = _baseHandlerSymbols[_baseHandlerSymbols.Length - 1];

            TryLoadBaseMessageSymbols(out _baseMessageSymbols, out _notificationInterfaceSymbol);
        }
        catch (Exception ex)
        {
            Action<Diagnostic> report = ReportDiagnostic;
            report.ReportGenericError(ex);
            HasErrors = true;
        }
    }

    private void TryLoadUnitSymbol(out INamedTypeSymbol UnitSymbol)
    {
        var unitSymbolName = $"{Constants.MediatorLib}.Unit";
        var unitSymbol = _context.Compilation.GetTypeByMetadataName(unitSymbolName)?.OriginalDefinition;
        if (unitSymbol is null)
        {
            UnitSymbol = null!;
            ReportDiagnostic(
                unitSymbolName,
                ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
            );
        }
        else
        {
            UnitSymbol = unitSymbol;
        }
    }

    private void TryLoadBaseHandlerSymbols(out INamedTypeSymbol[] _baseHandlerSymbols)
    {
        var baseHandlerSymbolNames = new[]
        {
            $"{Constants.MediatorLib}.IRequestHandler`1",
            $"{Constants.MediatorLib}.IRequestHandler`2",
            $"{Constants.MediatorLib}.IStreamRequestHandler`2",
            $"{Constants.MediatorLib}.ICommandHandler`1",
            $"{Constants.MediatorLib}.ICommandHandler`2",
            $"{Constants.MediatorLib}.IStreamCommandHandler`2",
            $"{Constants.MediatorLib}.IQueryHandler`2",
            $"{Constants.MediatorLib}.IStreamQueryHandler`2",
            $"{Constants.MediatorLib}.INotificationHandler`1",
        };

        var baseHandlerSymbols = baseHandlerSymbolNames
            .Select(n => (Name: n, Symbol: _context.Compilation.GetTypeByMetadataName(n)?.OriginalDefinition))
            .ToArray();

        _baseHandlerSymbols = Array.Empty<INamedTypeSymbol>();
        if (baseHandlerSymbols.Any(s => s.Symbol is null))
        {
            foreach (var (name, symbol) in baseHandlerSymbols)
            {
                if (symbol is not null)
                    continue;
                ReportDiagnostic(
                    name,
                    ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
                );
            }
        }
        else
        {
            _baseHandlerSymbols = baseHandlerSymbols.Select(s => s.Symbol!).ToArray();
        }
    }

    private void TryLoadDISymbols(
        out INamedTypeSymbol _serviceLifetimeEnumSymbol,
        out IFieldSymbol SingletonServiceLifetimeSymbol
    )
    {
        var serviceLifetimeEnumSymbolName = "Microsoft.Extensions.DependencyInjection.ServiceLifetime";
        var serviceLifetimeEnumSymbol = _context.Compilation.GetTypeByMetadataName(serviceLifetimeEnumSymbolName);
        if (serviceLifetimeEnumSymbol is null)
        {
            _serviceLifetimeEnumSymbol = null!;
            SingletonServiceLifetimeSymbol = null!;
            ReportDiagnostic(
                serviceLifetimeEnumSymbolName,
                ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
            );
        }
        else
        {
            _serviceLifetimeEnumSymbol = serviceLifetimeEnumSymbol;
            SingletonServiceLifetimeSymbol = (IFieldSymbol)_serviceLifetimeEnumSymbol
                .GetMembers()
                .Single(m => m.Name == "Singleton");
        }
    }

    private void TryLoadBaseMessageSymbols(
        out INamedTypeSymbol[] _baseMessageSymbols,
        out INamedTypeSymbol _notificationInterfaceSymbol
    )
    {
        var baseMessageSymbolNames = new[]
        {
            $"{Constants.MediatorLib}.IRequest",
            $"{Constants.MediatorLib}.IRequest`1",
            $"{Constants.MediatorLib}.IStreamRequest`1",
            $"{Constants.MediatorLib}.ICommand",
            $"{Constants.MediatorLib}.ICommand`1",
            $"{Constants.MediatorLib}.IStreamCommand`1",
            $"{Constants.MediatorLib}.IQuery`1",
            $"{Constants.MediatorLib}.IStreamQuery`1",
            $"{Constants.MediatorLib}.INotification",
        };
        var baseMessageSymbols = baseMessageSymbolNames
            .Select(n => (Name: n, Symbol: _context.Compilation.GetTypeByMetadataName(n)?.OriginalDefinition))
            .ToArray();

        _baseMessageSymbols = Array.Empty<INamedTypeSymbol>();
        if (baseMessageSymbols.Any(s => s.Symbol is null))
        {
            _notificationInterfaceSymbol = null!;
            foreach (var (name, symbol) in baseMessageSymbols)
            {
                if (symbol is not null)
                    continue;
                ReportDiagnostic(
                    name,
                    ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
                );
            }
        }
        else
        {
            _baseMessageSymbols = baseMessageSymbols.Select(s => s.Symbol!).ToArray();
            _notificationInterfaceSymbol = _baseMessageSymbols[_baseMessageSymbols.Length - 1];
        }
    }

    public void Analyze()
    {
        if (HasErrors)
            return;

        try
        {
            var queue = new Queue<INamespaceOrTypeSymbol>();

            FindGlobalNamespaces(queue);

            PopulateMetadata(queue);
        }
        catch (Exception ex)
        {
            _context.ReportGenericError(ex);
            HasErrors = true;
        }
    }

    public CompilationModel ToModel()
    {
        var model = new CompilationModel(
            _requestMessages
                .Select(
                    x =>
                        new RequestMessageModel(
                            x.Symbol,
                            x.ResponseSymbol,
                            x.MessageType,
                            x.Handler?.ToModel(),
                            x.WrapperType
                        )
                )
                .ToImmutableEquatableArray(),
            _notificationMessages.Select(x => x.ToModel()).ToImmutableEquatableArray(),
            _requestMessageHandlers.Select(x => x.ToModel()).ToImmutableEquatableArray(),
            _notificationMessageHandlers.Select(x => x.ToModel()).ToImmutableEquatableArray(),
            RequestMessageHandlerWrappers.ToImmutableEquatableArray(),
            HasErrors,
            MediatorNamespace,
            GeneratorVersion,
            ServiceLifetime,
            ServiceLifetimeShort,
            SingletonServiceLifetime,
            IsTestRun,
            ConfiguredViaAttribute,
            ConfiguredViaConfiguration
        );

        return model;
    }

    private void FindGlobalNamespaces(Queue<INamespaceOrTypeSymbol> queue)
    {
        var compilation = _context.Compilation;

        queue.Enqueue(compilation.Assembly.GlobalNamespace);

        var markerSymbol = UnitSymbol;
        if (markerSymbol is null)
            throw new Exception("Can't load marker symbol");

        var assemblyCache = new Dictionary<IModuleSymbol, ImmutableArray<IAssemblySymbol>>(
            SymbolEqualityComparer.Default
        );

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                continue;

            if (_symbolComparer.Equals(markerSymbol.ContainingAssembly, assemblySymbol))
                continue;
            if (assemblySymbol.Name.StartsWith("Mediator.SourceGenerator", StringComparison.Ordinal))
                continue;

            if (!assemblySymbol.Modules.Any(m => IsMediatorLibReferencedByTheModule(m, assemblyCache)))
                continue;

            queue.Enqueue(assemblySymbol.GlobalNamespace);
        }
    }

    private static bool IsMediatorLibReferencedByTheModule(
        IModuleSymbol module,
        Dictionary<IModuleSymbol, ImmutableArray<IAssemblySymbol>> assemblyCache
    )
    {
        const int maxDepth = 3;

        // Create a queue for breadth-first traversal.
        var moduleQueue = new Queue<IModuleSymbol>();

        // Create a set to keep track of visited modules.
        var visited = new HashSet<IModuleSymbol>(SymbolEqualityComparer.Default);

        // Enqueue the initial module for processing.
        moduleQueue.Enqueue(module);
        visited.Add(module);

        var depth = 0;

        while (moduleQueue.Count > 0)
        {
            var count = moduleQueue.Count;

            // Process modules at the current depth level.
            for (var i = 0; i < count; i++)
            {
                var currentModule = moduleQueue.Dequeue();

                // Check if the current module references MediatorLib.
                if (currentModule.ReferencedAssemblies.Any(ra => ra.Name == Constants.MediatorLib))
                {
                    return true;
                }

                // Above we've checked for direct dependencies on the Mediator.Abstractions package,
                // but projects can implement Mediator messages using only a transitive dependency
                // as well.
                // Even so, going too deep recursively will severely impact build performance
                // so for now the max depth is limited to 3.
                // This should be solved properly by changing how codegen works..

                // Access cached assemblies.
                if (!assemblyCache.TryGetValue(currentModule, out var assemblies))
                {
                    assemblies = currentModule.ReferencedAssemblySymbols;
                    assemblyCache[currentModule] = assemblies;
                }

                // Enqueue referenced modules for the next depth level.
                foreach (var assembly in assemblies)
                {
                    foreach (var referencedModule in assembly.Modules)
                    {
                        if (visited.Add(referencedModule))
                        {
                            moduleQueue.Enqueue(referencedModule);
                        }
                    }
                }
            }

            depth++;

            // Limit the max depth.
            if (depth > maxDepth)
            {
                break;
            }
        }

        return false;
    }

    private void PopulateMetadata(Queue<INamespaceOrTypeSymbol> queue)
    {
        var compilation = _context.Compilation;

        if (UnitSymbol is null)
            return;

#pragma warning disable RS1024 // Compare symbols correctly
        var requestMessageHandlerMap = new Dictionary<INamedTypeSymbol, object?>(_symbolComparer);
#pragma warning restore RS1024 // Compare symbols correctly

        while (queue.Count > 0)
        {
            var nsOrTypeSymbol = queue.Dequeue();

            if (nsOrTypeSymbol is INamespaceSymbol nsSymbol)
                foreach (var member in nsSymbol.GetMembers())
                    ProcessMember(queue, member, requestMessageHandlerMap);
            else
                ProcessMember(queue, (INamedTypeSymbol)nsOrTypeSymbol, requestMessageHandlerMap);
        }

        foreach (var kvp in requestMessageHandlerMap)
        {
            if (kvp.Value is not RequestMessage message)
                continue;

            ReportDiagnostic(
                message.Symbol,
                (in CompilationAnalyzerContext c, INamedTypeSymbol s) => c.ReportMessageWithoutHandler(s)
            );
        }

        foreach (var notificationMessage in _notificationMessages)
        {
            var handlerInterface = _baseHandlerSymbols[_baseHandlerSymbols.Length - 1].Construct(
                notificationMessage.Symbol
            );

            foreach (var notificationMessageHandler in _notificationMessageHandlers)
            {
                if (notificationMessageHandler.IsOpenGeneric) // These are added as open generics
                    continue;

                if (compilation.HasImplicitConversion(notificationMessageHandler.Symbol, handlerInterface))
                    notificationMessage.AddHandlers(notificationMessageHandler);
            }

            // This diagnostic is not safe to use here.
            // A user can define a notification, expecting it to only
            // show up in an open generic handler.
            // We don't keep track of bindings between notification and
            // these open generic handlers, so we can't know what notifications
            // are and aren't handled just yet.
            // TODO - include open generic handlers in analysis as well, so that we can report this correctly.
            //if (notificationMessage.HandlerCount == 0)
            //{
            //    ReportDiagnostic(notificationMessage.Symbol, (in GeneratorExecutionContext c, INamedTypeSymbol s) => c.ReportMessageWithoutHandler(s));
            //}
        }

        const int NOT_RELEVANT = 0;
        const int IS_REQUEST_HANDLER = 1;
        const int IS_NOTIFICATION_HANDLER = 2;
        const int IS_REQUEST = 3;
        const int IS_NOTIFICATION = 4;

        void ProcessMember(
            Queue<INamespaceOrTypeSymbol> queue,
            INamespaceOrTypeSymbol member,
            Dictionary<INamedTypeSymbol, object?> mapping
        )
        {
            if (member is INamespaceSymbol childNsSymbol)
            {
                queue.Enqueue(childNsSymbol);
                return;
            }

            var typeSymbol = (INamedTypeSymbol)member;

            foreach (var childTypeSymbol in typeSymbol.GetTypeMembers())
                queue.Enqueue(childTypeSymbol);

            if (typeSymbol.IsStatic || typeSymbol.IsAbstract)
                return;

            var isStruct = typeSymbol.TypeKind == TypeKind.Struct;
            if (!isStruct && typeSymbol.TypeKind != TypeKind.Class)
                return;

            var typeInterfaces = typeSymbol.AllInterfaces;
            for (int i = 0; i < typeInterfaces.Length; i++)
            {
                var typeInterfaceSymbol = typeInterfaces[i];

                if (typeInterfaceSymbol.ContainingNamespace.Name != Constants.MediatorLib)
                    continue;

                if (!ProcessInterface(i, typeSymbol, typeInterfaceSymbol, isStruct, mapping))
                    break;
            }
        }

        bool ProcessInterface(
            int i,
            INamedTypeSymbol typeSymbol,
            INamedTypeSymbol typeInterfaceSymbol,
            bool isStruct,
            Dictionary<INamedTypeSymbol, object?> mapping
        )
        {
            var codeOfInterest = IsInteresting(typeInterfaceSymbol);
            switch (codeOfInterest)
            {
                case NOT_RELEVANT:
                    break; // Continue loop
                case IS_REQUEST_HANDLER:
                case IS_NOTIFICATION_HANDLER:

                    {
                        if (isStruct)
                        {
                            // Handlers must be classes
                            ReportDiagnostic(
                                typeSymbol,
                                (in CompilationAnalyzerContext c, INamedTypeSymbol s) => c.ReportInvalidHandlerType(s)
                            );
                            return false;
                        }

                        // TODO, return here? AllInterfaces ordered?
                        if (IsAlreadyHandledByDerivedInterface(i, 1, typeSymbol, typeInterfaceSymbol))
                            break;

                        var isRequest = codeOfInterest == IS_REQUEST_HANDLER;

                        // TODO: invert if
                        if (isRequest)
                        {
                            if (IsOpenGeneric(typeSymbol))
                            {
                                // Handlers must be classes
                                ReportDiagnostic(
                                    typeSymbol,
                                    (in CompilationAnalyzerContext c, INamedTypeSymbol s) =>
                                        c.ReportOpenGenericRequestHandler(s)
                                );
                                return false;
                            }

                            var messageType = typeInterfaceSymbol.Name.Substring(
                                1,
                                typeInterfaceSymbol.Name.IndexOf("Handler") - 1
                            );

                            var handler = new RequestMessageHandler(typeSymbol, messageType, this);
                            var requestMessageSymbol = (INamedTypeSymbol)typeInterfaceSymbol.TypeArguments[0];
                            if (mapping.TryGetValue(requestMessageSymbol, out var requestMessageObj))
                            {
                                if (requestMessageObj is not RequestMessage requestMessage)
                                {
                                    // Signal that we have duplicates
                                    ReportDiagnostic(
                                        typeSymbol,
                                        (in CompilationAnalyzerContext c, INamedTypeSymbol s) =>
                                            c.ReportMultipleHandlers(s)
                                    );
                                    return false;
                                }
                                mapping[requestMessageSymbol] = null;
                                requestMessage.SetHandler(handler);
                            }
                            else
                            {
                                mapping.Add(requestMessageSymbol, handler);
                            }

                            _requestMessageHandlers.Add(handler);
                        }
                        else
                        {
                            _notificationMessageHandlers.Add(new NotificationMessageHandler(typeSymbol, this));
                        }
                    }
                    break;
                case IS_REQUEST:

                    {
                        ITypeSymbol responseMessageSymbol =
                            typeInterfaceSymbol.TypeArguments.Length > 0
                                ? typeInterfaceSymbol.TypeArguments[0]
                                : UnitSymbol;

                        if (IsAlreadyHandledByDerivedInterface(i, 0, typeSymbol, typeInterfaceSymbol))
                            break;

                        var messageType =
                            typeInterfaceSymbol.Name.IndexOf('<') == -1
                                ? typeInterfaceSymbol.Name.Substring(1)
                                : typeInterfaceSymbol.Name.Substring(1, typeInterfaceSymbol.Name.IndexOf('<') - 1);

                        var message = new RequestMessage(typeSymbol, responseMessageSymbol, messageType, this);
                        if (!_requestMessages.Add(message))
                        {
                            // If this symbol has already been added,
                            // the type implements multiple base message types.
                            ReportDiagnostic(
                                typeSymbol,
                                (in CompilationAnalyzerContext c, INamedTypeSymbol s) =>
                                    c.ReportMessageDerivesFromMultipleMessageInterfaces(s)
                            );
                            return false;
                        }
                        else
                        {
                            if (mapping.TryGetValue(typeSymbol, out var requestMessageHandlerObj))
                            {
                                mapping[typeSymbol] = null;
                                message.SetHandler((RequestMessageHandler)requestMessageHandlerObj!);
                            }
                            else
                            {
                                mapping.Add(typeSymbol, message);
                            }
                        }
                    }
                    break;
                case IS_NOTIFICATION:

                    {
                        if (!_notificationMessages.Add(new NotificationMessage(typeSymbol, this)))
                        {
                            // If this symbol has already been added,
                            // the type implements multiple base message types.
                            ReportDiagnostic(
                                typeSymbol,
                                (in CompilationAnalyzerContext c, INamedTypeSymbol s) =>
                                    c.ReportMessageDerivesFromMultipleMessageInterfaces(s)
                            );
                            return false;
                        }
                    }
                    break;
            }

            return true;
        }

        int IsInteresting(INamedTypeSymbol interfaceSymbol)
        {
            var originalInterfaceSymbol = interfaceSymbol.OriginalDefinition;

            for (int i = 0; i < _baseHandlerSymbols.Length; i++)
            {
                var baseSymbol = _baseHandlerSymbols[i];
                if (_symbolComparer.Equals(baseSymbol, originalInterfaceSymbol))
                    return _symbolComparer.Equals(baseSymbol, _notificationHandlerInterfaceSymbol)
                      ? IS_NOTIFICATION_HANDLER
                      : IS_REQUEST_HANDLER;
            }

            for (int i = 0; i < _baseMessageSymbols.Length; i++)
            {
                var baseSymbol = _baseMessageSymbols[i];
                if (_symbolComparer.Equals(baseSymbol, originalInterfaceSymbol))
                    return _symbolComparer.Equals(baseSymbol, _notificationInterfaceSymbol)
                      ? IS_NOTIFICATION
                      : IS_REQUEST;
            }

            return NOT_RELEVANT;
        }

        bool IsAlreadyHandledByDerivedInterface(
            int i,
            int responseTypeArgumentIndex,
            INamedTypeSymbol typeSymbol,
            INamedTypeSymbol typeInterfaceSymbol
        )
        {
            var mightSkip =
                typeInterfaceSymbol.TypeArguments.Length > responseTypeArgumentIndex
                && typeInterfaceSymbol.TypeArguments[responseTypeArgumentIndex] is INamedTypeSymbol responseTypeSymbol
                && _symbolComparer.Equals(responseTypeSymbol, UnitSymbol);

            if (!mightSkip)
                return false;

            for (int j = 0; j < i; j++)
            {
                var prevTypeInterfaceSymbol = typeSymbol.AllInterfaces[j];

                if (prevTypeInterfaceSymbol.ContainingNamespace.Name != Constants.MediatorLib)
                    continue;

                if (prevTypeInterfaceSymbol.Interfaces.Contains(typeInterfaceSymbol, _symbolComparer))
                    return true;
            }

            return false;
        }
    }

    static bool IsOpenGeneric(INamedTypeSymbol symbol) =>
        symbol.TypeArguments.Length > 0 && symbol.TypeArguments[0] is ITypeParameterSymbol;

    private void TryParseConfiguration()
    {
        var cancellationToken = _context.CancellationToken;
        var compilation = _context.Compilation;
        var addMediatorCalls = _context.AddMediatorCalls;

        //System.Diagnostics.Debugger.Launch();

        var configuredByAddMediator = false;
        if (addMediatorCalls is not null && addMediatorCalls.Count > 0)
        {
            ProcessAddMediatorConfiguration(addMediatorCalls, ref configuredByAddMediator, cancellationToken);
        }

        var attrs = compilation.Assembly.GetAttributes();
        var optionsAttr = attrs.SingleOrDefault(
            a =>
            {
                if (a.AttributeClass is null)
                    return false;
                var attributeFullName = a.AttributeClass.GetTypeSymbolFullName(withGlobalPrefix: false);
                return attributeFullName == "Mediator.MediatorOptionsAttribute"
                    || attributeFullName == "MediatorOptions";
            }
        );
        if (optionsAttr is not null)
            ProcessAttributeConfiguration(optionsAttr, configuredByAddMediator, cancellationToken);
    }

    private void ProcessAddMediatorConfiguration(
        IReadOnlyList<InvocationExpressionSyntax> addMediatorCalls,
        ref bool configuredByAddMediator,
        CancellationToken cancellationToken
    )
    {
        var compilation = _context.Compilation;

        foreach (var addMediatorCall in addMediatorCalls)
        {
            var semanticModel = compilation.GetSemanticModel(addMediatorCall.SyntaxTree);

            var symbol = semanticModel.GetSymbolInfo(addMediatorCall).Symbol;
            if (symbol is not null)
            {
                // This is actually expected not to hit,
                // since the AddMediator method is generated after this analysis runs.
                // So if we resolve a symbol here, it is likely from some other library
                // so we need to check that the symbol name matches our expectations.

                var assembly = symbol.ContainingAssembly;
                if (!_symbolComparer.Equals(_context.Compilation.Assembly, assembly))
                    continue;

                var containingType = symbol.ContainingType.GetTypeSymbolFullName(withGlobalPrefix: false);
                const string expectedGeneratedType =
                    "Microsoft.Extensions.DependencyInjection.MediatorDependencyInjectionExtensions";

                if (containingType != expectedGeneratedType)
                    continue;
            }

            if (addMediatorCall.ArgumentList.Arguments.Count == 0)
                continue;

            var wasAlreadyConfigured = configuredByAddMediator;

            configuredByAddMediator = true;
            var lifetimeArgument = addMediatorCall.ArgumentList.Arguments.Last();
            if (
                lifetimeArgument.Expression
                is not SimpleLambdaExpressionSyntax
                    and not ParenthesizedLambdaExpressionSyntax
            )
            {
                ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
                continue;
            }

            var lambda = (LambdaExpressionSyntax)lifetimeArgument.Expression;

            if (
                lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda
                && parenthesizedLambda.ParameterList.Parameters.Count != 1
            )
                continue;

            var parameter = lambda switch
            {
                SimpleLambdaExpressionSyntax f => f.Parameter,
                ParenthesizedLambdaExpressionSyntax f => f.ParameterList.Parameters[0],
                _ => throw new Exception("Invalid state"),
            };

            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);
            if (parameterSymbol is not null)
            {
                var parameterTypeName = parameterSymbol.Type.GetTypeSymbolFullName(false);
                // Parameter type name will be empty string if it can't be resolved
                if (parameterTypeName != "" && parameterTypeName != "Mediator.MediatorOptions")
                    continue;
            }

            var body = lambda.Body;

            if (body is AssignmentExpressionSyntax simpleAssignment)
            {
                if (!ProcessAddMediatorAssignmentStatement(simpleAssignment, semanticModel, cancellationToken))
                    continue;
            }
            else if (body is BlockSyntax block)
            {
                foreach (var statement in block.Statements)
                {
                    if (statement is not ExpressionStatementSyntax statementExpression)
                    {
                        ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
                        break;
                    }
                    if (statementExpression.Expression is not AssignmentExpressionSyntax assignment)
                    {
                        ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
                        break;
                    }
                    if (!ProcessAddMediatorAssignmentStatement(assignment, semanticModel, cancellationToken))
                        break;
                }
            }
            else
            {
                ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
            }
        }
    }

    private void ProcessAttributeConfiguration(
        AttributeData optionsAttr,
        bool configuredByAddMediator,
        CancellationToken cancellationToken
    )
    {
        var compilation = _context.Compilation;

        if (configuredByAddMediator)
        {
            ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportConflictingConfiguration());
        }

        var syntaxReference = optionsAttr.ApplicationSyntaxReference;
        if (syntaxReference is null)
            return;

        var semanticModel = compilation.GetSemanticModel(syntaxReference.SyntaxTree);

        var optionsAttrSyntax = optionsAttr.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;
        if (optionsAttrSyntax is null || optionsAttrSyntax.ArgumentList is null)
            return;

        foreach (var attrArg in optionsAttrSyntax.ArgumentList.Arguments)
        {
            if (attrArg.NameEquals is null)
                throw new Exception("Error parsing MediatorOptions");

            var attrFieldName = attrArg.NameEquals.Name.ToString();
            if (attrFieldName == "ServiceLifetime")
            {
                var identifierNameSyntax = (IdentifierNameSyntax)(
                    (MemberAccessExpressionSyntax)attrArg.Expression
                ).Name;
                _configuredLifetimeSymbol = GetServiceLifetimeSymbol(
                    identifierNameSyntax,
                    semanticModel,
                    cancellationToken
                );
            }
            else if (attrFieldName == "Namespace")
            {
                var namespaceArg =
                    semanticModel.GetConstantValue(attrArg.Expression, cancellationToken).Value as string;
                if (!string.IsNullOrWhiteSpace(namespaceArg))
                    MediatorNamespace = namespaceArg!;
            }
        }

        ConfiguredViaAttribute = true;
    }

    private bool ProcessAddMediatorAssignmentStatement(
        AssignmentExpressionSyntax assignment,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        var opt = ((MemberAccessExpressionSyntax)assignment.Left).Name.Identifier.Text;
        if (opt == "Namespace")
        {
            if (assignment.Right is LiteralExpressionSyntax literal)
            {
                if (!literal.IsKind(SyntaxKind.NullLiteralExpression))
                    MediatorNamespace = literal.Token.ValueText;
            }
            else if (assignment.Right is IdentifierNameSyntax identifier)
            {
                var configuredNamespace = TryResolveNamespaceIdentifier(identifier, semanticModel, cancellationToken);
                if (configuredNamespace is not null)
                {
                    MediatorNamespace = configuredNamespace;
                }
                else
                {
                    ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
                    return false;
                }
            }
            else
            {
                ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
                return false;
            }
        }
        else if (opt == "ServiceLifetime")
        {
            if (assignment.Right is MemberAccessExpressionSyntax enumAccess)
            {
                var identifierNameSyntax = (IdentifierNameSyntax)enumAccess.Name;
                _configuredLifetimeSymbol = GetServiceLifetimeSymbol(
                    identifierNameSyntax,
                    semanticModel,
                    cancellationToken
                );
            }
            else if (assignment.Right is IdentifierNameSyntax identifier)
            {
                _configuredLifetimeSymbol = GetServiceLifetimeSymbol(identifier, semanticModel, cancellationToken);
            }
            else
            {
                ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
                return false;
            }
        }
        else
        {
            ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
            return false;
        }

        ConfiguredViaConfiguration = true;
        return true;

        string? TryResolveNamespaceIdentifier(
            IdentifierNameSyntax identifier,
            SemanticModel semanticModel,
            CancellationToken cancellationToken
        )
        {
            var variableSymbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
            return variableSymbol switch
            {
                null => null,
                IFieldSymbol fieldSymbol => TryResolveNamespaceSymbol(fieldSymbol, semanticModel, cancellationToken),
                IPropertySymbol propertySymbol
                  => TryResolveNamespaceSymbol(propertySymbol, semanticModel, cancellationToken),
                ILocalSymbol localSymbol => TryResolveNamespaceSymbol(localSymbol, semanticModel, cancellationToken),
                _ => null
            };
        }

        string? TryResolveNamespaceSymbol(
            ISymbol symbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken
        )
        {
            if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.HasConstantValue)
                return fieldSymbol.ConstantValue as string;
            if (symbol is ILocalSymbol localSymbol && localSymbol.HasConstantValue)
                return localSymbol.ConstantValue as string;

            var syntaxReferences = symbol.DeclaringSyntaxReferences;
            var syntaxNode = syntaxReferences.First().GetSyntax(cancellationToken);

            var initializerExpression = syntaxNode is VariableDeclaratorSyntax variableDeclarator
                ? variableDeclarator.Initializer?.Value
                : syntaxNode is PropertyDeclarationSyntax propertyDeclaration
                    ? propertyDeclaration.Initializer?.Value
                    : null;

            if (initializerExpression is null)
            {
                ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
                return null;
            }

            if (initializerExpression is LiteralExpressionSyntax literal)
                return literal.Token.ValueText;

            if (initializerExpression is IdentifierNameSyntax reference)
                return TryResolveNamespaceIdentifier(reference, semanticModel, cancellationToken);

            return null;
        }
    }

    private IFieldSymbol? GetServiceLifetimeSymbol(
        IdentifierNameSyntax identifierNameSyntax,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        var symbol = semanticModel.GetSymbolInfo(identifierNameSyntax, cancellationToken).Symbol;
        if (
            symbol is IFieldSymbol lifetimeSymbol
            && _serviceLifetimeEnumSymbol is not null
            && SymbolEqualityComparer.Default.Equals(_serviceLifetimeEnumSymbol, lifetimeSymbol.ContainingType)
        )
            return lifetimeSymbol;

        return symbol switch
        {
            IFieldSymbol fieldSymbol => TryGetServiceLifetimeSymbol(fieldSymbol, semanticModel, cancellationToken),
            IPropertySymbol propertySymbol
              => TryGetServiceLifetimeSymbol(propertySymbol, semanticModel, cancellationToken),
            ILocalSymbol localSymbol => TryGetServiceLifetimeSymbol(localSymbol, semanticModel, cancellationToken),
            _ => null
        };
    }

    private IFieldSymbol? TryGetServiceLifetimeSymbol(
        ISymbol symbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.HasConstantValue)
        {
            var value = (int)fieldSymbol.ConstantValue!;
            return _serviceLifetimeEnumSymbol
                ?.GetMembers()
                .OfType<IFieldSymbol>()
                .Single(m => (int)m.ConstantValue! == value);
        }
        if (symbol is ILocalSymbol localSymbol && localSymbol.HasConstantValue)
        {
            var value = (int)localSymbol.ConstantValue!;
            return _serviceLifetimeEnumSymbol
                ?.GetMembers()
                .OfType<IFieldSymbol>()
                .Single(m => (int)m.ConstantValue! == value);
        }

        var syntaxReferences = symbol.DeclaringSyntaxReferences;
        var syntaxNode = syntaxReferences.First().GetSyntax(cancellationToken);

        var initializerExpression = syntaxNode is VariableDeclaratorSyntax variableDeclarator
            ? variableDeclarator.Initializer?.Value
            : syntaxNode is PropertyDeclarationSyntax propertyDeclaration
                ? propertyDeclaration.Initializer?.Value
                : null;

        if (initializerExpression is null)
        {
            ReportDiagnostic((in CompilationAnalyzerContext c) => c.ReportInvalidCodeBasedConfiguration());
            return null;
        }

        if (initializerExpression is LiteralExpressionSyntax literal)
        {
            var value = (int)literal.Token.Value!;
            return _serviceLifetimeEnumSymbol
                ?.GetMembers()
                .OfType<IFieldSymbol>()
                .Single(m => (int)m.ConstantValue! == value);
        }

        if (initializerExpression is MemberAccessExpressionSyntax memberAccess)
            return GetServiceLifetimeSymbol((IdentifierNameSyntax)memberAccess.Name, semanticModel, cancellationToken);

        if (initializerExpression is IdentifierNameSyntax reference)
            return GetServiceLifetimeSymbol(reference, semanticModel, cancellationToken);

        return null;
    }

    private delegate Diagnostic ReportDiagnosticDelegate<T>(in CompilationAnalyzerContext context, T state);

    private void ReportDiagnostic(Diagnostic diagnostic)
    {
        _context.ReportDiagnostic(diagnostic);
    }

    private void ReportDiagnostic<T>(T state, ReportDiagnosticDelegate<T> del)
    {
        var diagnostic = del(in _context, state);
        HasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
    }

    private delegate Diagnostic ReportDiagnosticDelegate(in CompilationAnalyzerContext context);

    private void ReportDiagnostic(ReportDiagnosticDelegate del)
    {
        var diagnostic = del(in _context);
        HasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
    }
}
