using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp;

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
    private readonly HashSet<NotificationMessageHandler> _notificationMessageHandlers;
    private readonly List<PipelineBehaviorType> _pipelineBehaviors;
    private Queue<INamespaceOrTypeSymbol>? _configuredAssemblies;

    public ImmutableArray<RequestMessageHandlerWrapperModel> RequestMessageHandlerWrappers;

    private INamedTypeSymbol[] _baseHandlerSymbols;
    private INamedTypeSymbol[] _baseMessageSymbols;
    private INamedTypeSymbol? _pipelineBehaviorInterfaceSymbol;
    private INamedTypeSymbol? _streamPipelineBehaviorInterfaceSymbol;

    private INamedTypeSymbol? _notificationPublisherInterfaceSymbol;
    private INamedTypeSymbol? _notificationHandlerInterfaceSymbol;
    private INamedTypeSymbol? _notificationInterfaceSymbol;

    private INamedTypeSymbol? _notificationPublisherImplementationSymbol;

    private INamedTypeSymbol? _unitSymbol;

    private IFieldSymbol? _configuredLifetimeSymbol;
    private INamedTypeSymbol? _serviceLifetimeEnumSymbol;
    private IFieldSymbol? _singletonServiceLifetimeSymbol;
    private IFieldSymbol? _serviceLifetimeSymbol => _configuredLifetimeSymbol ?? _singletonServiceLifetimeSymbol;

    private IFieldSymbol? _configuredCachingModeSymbol;
    private INamedTypeSymbol? _cachingModeEnumSymbol;
    private IFieldSymbol? _eagerCachingModeSymbol;
    private IFieldSymbol? _cachingModeSymbol => _configuredCachingModeSymbol ?? _eagerCachingModeSymbol;

    private bool _configuredLazyCachingModeFromSyntax = false;

    private bool _hasErrors;
    private bool _isInitialized;

    [MemberNotNullWhen(
        true,
        nameof(_pipelineBehaviorInterfaceSymbol),
        nameof(_streamPipelineBehaviorInterfaceSymbol),
        nameof(_notificationPublisherInterfaceSymbol),
        nameof(_notificationHandlerInterfaceSymbol),
        nameof(_notificationInterfaceSymbol),
        nameof(_notificationPublisherImplementationSymbol),
        nameof(_unitSymbol),
        nameof(_serviceLifetimeEnumSymbol),
        nameof(_singletonServiceLifetimeSymbol),
        nameof(_serviceLifetimeSymbol)
    )]
    public bool InitializedSuccessfully => _isInitialized && !_hasErrors;

    public Compilation Compilation => _context.Compilation;

    public string MediatorNamespace { get; private set; } = Constants.MediatorLib;

    private string GeneratorVersion =>
        string.IsNullOrWhiteSpace(_context.GeneratorVersion) ? "1.0.0.0" : _context.GeneratorVersion;

    public string? ServiceLifetime => _serviceLifetimeSymbol?.GetFieldSymbolFullName();
    private string? ServiceLifetimeShort => _serviceLifetimeSymbol?.Name;

    private string? SingletonServiceLifetime => _singletonServiceLifetimeSymbol?.GetFieldSymbolFullName();

    public bool ServiceLifetimeIsSingleton => _serviceLifetimeSymbol?.Name == "Singleton";

    public bool ServiceLifetimeIsScoped => _serviceLifetimeSymbol?.Name == "Scoped";

    public bool ServiceLifetimeIsTransient => _serviceLifetimeSymbol?.Name == "Transient";

    public string? CachingMode => _cachingModeSymbol?.GetFieldSymbolFullName();
    private string? CachingModeShort =>
        _cachingModeSymbol?.Name ?? (_configuredLazyCachingModeFromSyntax ? "Lazy" : null);

    public bool CachingModeIsEager =>
        _cachingModeSymbol?.Name == "Eager" || (!_configuredLazyCachingModeFromSyntax && _cachingModeSymbol == null);

    public bool CachingModeIsLazy => _cachingModeSymbol?.Name == "Lazy" || _configuredLazyCachingModeFromSyntax;

    private bool IsTestRun =>
        (_context.Compilation.AssemblyName?.StartsWith("Mediator.Tests") ?? false)
        || (_context.Compilation.AssemblyName?.StartsWith("Mediator.SmokeTest") ?? false);

    public bool GenerateTypesAsInternal { get; private set; }

    private bool ConfiguredViaAttribute { get; set; }

    private bool ConfiguredViaConfiguration { get; set; }

    private int MessageCountThreshold
    {
        get
        {
            var symbolNames = new HashSet<string>(
                _context
                    .Compilation.SyntaxTrees.Select(tree => tree.Options as CSharpParseOptions)
                    .Where(options => options is not null)
                    .SelectMany(options => options!.PreprocessorSymbolNames)
            );

            // If we compile using Mediator_Default_Project it means
            // we've parameterized test run between
            // - Mediator_Default_Project
            // - Mediator_Large_Project
            // where the intention is to have tests cover both paths
            // in terms of the differences in generated code
            if (symbolNames.Contains("Mediator_Default_Project"))
                return 100_000;

            // Found during benchmarking in .NET 8/3.0.0 version
            // of this library
            return 16;
        }
    }

    public CompilationAnalyzer(in CompilationAnalyzerContext context)
    {
        _context = context;
        _requestMessages = new();
        _notificationMessages = new();
        _notificationMessageHandlers = new();
        _pipelineBehaviors = new();
        _baseHandlerSymbols = Array.Empty<INamedTypeSymbol>();
        _baseMessageSymbols = Array.Empty<INamedTypeSymbol>();
    }

    public void Initialize()
    {
        try
        {
            TryLoadUnitSymbol(out _unitSymbol);

            TryLoadBaseHandlerSymbols(out _baseHandlerSymbols);

            TryLoadDISymbols(out _serviceLifetimeEnumSymbol, out _singletonServiceLifetimeSymbol);

            TryLoadCachingModeSymbols(out _cachingModeEnumSymbol, out _eagerCachingModeSymbol);

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

            TryLoadBaseMessageSymbols(out _baseMessageSymbols, out _notificationInterfaceSymbol);
        }
        catch (Exception ex)
        {
            Action<Diagnostic> report = ReportDiagnostic;
            report.ReportGenericError(ex);
            _hasErrors = true;
        }
        finally
        {
            _isInitialized = true;
        }
    }

    private void TryLoadUnitSymbol(out INamedTypeSymbol? unitSymbol)
    {
        var unitSymbolName = $"{Constants.MediatorLib}.Unit";
        var readUnitSymbol = _context.Compilation.GetTypeByMetadataName(unitSymbolName)?.OriginalDefinition;
        if (readUnitSymbol is null)
        {
            unitSymbol = null;
            ReportDiagnostic(
                unitSymbolName,
                ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
            );
        }
        else
        {
            unitSymbol = readUnitSymbol;
        }
    }

    private void TryLoadBaseHandlerSymbols(out INamedTypeSymbol[] baseHandlerSymbols)
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

        baseHandlerSymbols = new INamedTypeSymbol[baseHandlerSymbolNames.Length];
        for (int i = 0; i < baseHandlerSymbolNames.Length; i++)
        {
            var name = baseHandlerSymbolNames[i];
            var symbol = ReadSymbol(name);
            if (symbol is null)
                continue;

            if (name.EndsWith(".INotificationHandler`1", StringComparison.Ordinal))
                _notificationHandlerInterfaceSymbol = symbol;

            baseHandlerSymbols[i] = symbol;
        }

        _pipelineBehaviorInterfaceSymbol = ReadSymbol($"{Constants.MediatorLib}.IPipelineBehavior`2");
        _streamPipelineBehaviorInterfaceSymbol = ReadSymbol($"{Constants.MediatorLib}.IStreamPipelineBehavior`2");

        _notificationPublisherInterfaceSymbol = ReadSymbol($"{Constants.MediatorLib}.INotificationPublisher");
        _notificationPublisherImplementationSymbol = ReadSymbol($"{Constants.MediatorLib}.ForeachAwaitPublisher");

        INamedTypeSymbol? ReadSymbol(string name)
        {
            var symbol = _context.Compilation.GetTypeByMetadataName(name)?.OriginalDefinition;
            if (symbol is null)
            {
                ReportDiagnostic(
                    name,
                    ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
                );
                return null;
            }

            return symbol;
        }
    }

    private void TryLoadDISymbols(
        out INamedTypeSymbol? serviceLifetimeEnumSymbol,
        out IFieldSymbol? singletonServiceLifetimeSymbol
    )
    {
        var serviceLifetimeEnumSymbolName = "Microsoft.Extensions.DependencyInjection.ServiceLifetime";
        var readServiceLifetimeEnumSymbol = _context.Compilation.GetTypeByMetadataName(serviceLifetimeEnumSymbolName);
        if (readServiceLifetimeEnumSymbol is null)
        {
            serviceLifetimeEnumSymbol = null;
            singletonServiceLifetimeSymbol = null;
            ReportDiagnostic(
                serviceLifetimeEnumSymbolName,
                ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
            );
        }
        else
        {
            serviceLifetimeEnumSymbol = readServiceLifetimeEnumSymbol;
            singletonServiceLifetimeSymbol = (IFieldSymbol)
                serviceLifetimeEnumSymbol.GetMembers().Single(m => m.Name == "Singleton");
        }
    }

    private void TryLoadCachingModeSymbols(
        out INamedTypeSymbol? cachingModeEnumSymbol,
        out IFieldSymbol? eagerCachingModeSymbol
    )
    {
        var cachingModeEnumSymbolName = $"{Constants.MediatorLib}.CachingMode";
        var readCachingModeEnumSymbol = _context.Compilation.GetTypeByMetadataName(cachingModeEnumSymbolName);
        if (readCachingModeEnumSymbol is null)
        {
            // The CachingMode enum is generated by this source generator,
            // so it won't be available on the first pass. This is not an error.
            cachingModeEnumSymbol = null;
            eagerCachingModeSymbol = null;
        }
        else
        {
            cachingModeEnumSymbol = readCachingModeEnumSymbol;
            eagerCachingModeSymbol = (IFieldSymbol)cachingModeEnumSymbol.GetMembers().Single(m => m.Name == "Eager");
        }
    }

    private void TryLoadBaseMessageSymbols(
        out INamedTypeSymbol[] baseMessageSymbols,
        out INamedTypeSymbol? notificationInterfaceSymbol
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

        baseMessageSymbols = new INamedTypeSymbol[baseMessageSymbolNames.Length];
        notificationInterfaceSymbol = null;
        for (int i = 0; i < baseMessageSymbolNames.Length; i++)
        {
            var name = baseMessageSymbolNames[i];
            var symbol = _context.Compilation.GetTypeByMetadataName(name)?.OriginalDefinition;
            if (symbol is null)
            {
                ReportDiagnostic(
                    name,
                    ((in CompilationAnalyzerContext c, string name) => c.ReportRequiredSymbolNotFound(name))
                );
                continue;
            }

            if (name.EndsWith(".INotification", StringComparison.Ordinal))
                notificationInterfaceSymbol = symbol;

            baseMessageSymbols[i] = symbol;
        }
    }

    public void Analyze()
    {
        if (!InitializedSuccessfully)
            return;

        try
        {
            var queue = _configuredAssemblies ?? new Queue<INamespaceOrTypeSymbol>();
            if (_configuredAssemblies is null)
            {
                FindGlobalNamespaces(queue);
            }

            PopulateMetadata(queue);
        }
        catch (Exception ex)
        {
            _context.ReportGenericError(ex);
            _hasErrors = true;
        }
    }

    private static ImmutableEquatableArray<TModel> ToModelsSortedByInheritanceDepth<TSource, TModel>(
        HashSet<TSource> source,
        Func<TSource, TModel> selector
    )
        where TSource : SymbolMetadata<TSource>
        where TModel : SymbolMetadataModel, IEquatable<TModel>
    {
        var analysis = new (TSource Message, int Depth)[source.Count];
        int i = 0;
        foreach (var message in source)
        {
            var baseType = message.Symbol.BaseType;
            int depth = 0;
            while (baseType is not null && baseType.SpecialType != SpecialType.System_Object)
            {
                depth++;
                baseType = baseType.BaseType;
            }

            Debug.Assert(i < source.Count);
            analysis[i++] = (message, depth);
        }

        Array.Sort(analysis, (x, y) => y.Depth.CompareTo(x.Depth));
        var models = new TModel[source.Count];
        for (i = 0; i < source.Count; i++)
            models[i] = selector(analysis[i].Message);

        return new ImmutableEquatableArray<TModel>(models);
    }

    public CompilationModel ToModel()
    {
        if (_hasErrors)
            return new CompilationModel(MediatorNamespace, GeneratorVersion);

        try
        {
            if (_notificationPublisherImplementationSymbol is null)
                throw new Exception("Unexpected state: NotificationPublisherImplementationSymbol is null");

            var model = new CompilationModel(
                ToModelsSortedByInheritanceDepth(
                    _requestMessages,
                    m => new RequestMessageModel(
                        m.Symbol,
                        m.ResponseSymbol,
                        m.MessageType,
                        m.Handler?.ToModel(),
                        m.WrapperType
                    )
                ),
                ToModelsSortedByInheritanceDepth(_notificationMessages, m => m.ToModel()),
                _notificationMessageHandlers.Select(x => x.ToModel()).ToImmutableEquatableArray(),
                RequestMessageHandlerWrappers.ToImmutableEquatableArray(),
                new NotificationPublisherTypeModel(
                    _notificationPublisherImplementationSymbol.GetTypeSymbolFullName(),
                    _notificationPublisherImplementationSymbol.Name
                ),
                _pipelineBehaviors.Select(x => x.ToModel()).ToImmutableEquatableArray(),
                _hasErrors,
                MediatorNamespace,
                GeneratorVersion,
                ServiceLifetime,
                ServiceLifetimeShort,
                SingletonServiceLifetime,
                IsTestRun,
                ConfiguredViaAttribute,
                GenerateTypesAsInternal,
                CachingMode,
                CachingModeShort,
                MessageCountThreshold
            );

            return model;
        }
        catch (Exception ex)
        {
            _context.ReportGenericError(ex);
            _hasErrors = true;

            return new CompilationModel(MediatorNamespace, GeneratorVersion);
        }
    }

    private void FindGlobalNamespaces(Queue<INamespaceOrTypeSymbol> queue)
    {
        if (!InitializedSuccessfully)
            return;
        var compilation = _context.Compilation;

        queue.Enqueue(compilation.Assembly.GlobalNamespace);

        var assemblyCache = new Dictionary<IModuleSymbol, ImmutableArray<IAssemblySymbol>>(
            SymbolEqualityComparer.Default
        );

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                continue;

            if (_symbolComparer.Equals(_unitSymbol.ContainingAssembly, assemblySymbol))
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
        if (!InitializedSuccessfully)
            return;

        var compilation = _context.Compilation;

        var requestMessageHandlerMap = new Dictionary<INamedTypeSymbol, object?>(_symbolComparer);

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
            var isHandled = false;
            foreach (var notificationMessageHandler in _notificationMessageHandlers)
                isHandled |= notificationMessageHandler.TryAddMessage(notificationMessage);

            if (!isHandled)
            {
                ReportDiagnostic(
                    notificationMessage.Symbol,
                    (in CompilationAnalyzerContext c, INamedTypeSymbol s) => c.ReportMessageWithoutHandler(s)
                );
            }
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
                        }
                        else
                        {
                            // If the type is the `NotificationHandlerWrapper` that is generated, we don't want to add it to the list
                            // It is specially registered to DI and invoked by the internals of the Mediator
                            if (
                                typeSymbol
                                    .GetAttributes()
                                    .Any(a =>
                                        a.AttributeClass?.Name == "GeneratedCodeAttribute"
                                        && a.ConstructorArguments.FirstOrDefault().Value is string name
                                        && name == "Mediator.SourceGenerator"
                                    )
                            )
                            {
                                return true;
                            }
                            _notificationMessageHandlers.Add(
                                new NotificationMessageHandler(typeSymbol, _notificationHandlerInterfaceSymbol, this)
                            );
                        }
                    }
                    break;
                case IS_REQUEST:
                    {
                        ITypeSymbol responseMessageSymbol =
                            typeInterfaceSymbol.TypeArguments.Length > 0
                                ? typeInterfaceSymbol.TypeArguments[0]
                                : _unitSymbol;

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

                            foreach (var pipelineBehaviorType in _pipelineBehaviors)
                                pipelineBehaviorType.TryAddMessage(message);
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
                && _symbolComparer.Equals(responseTypeSymbol, _unitSymbol);

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
        var optionsAttr = attrs.SingleOrDefault(a =>
        {
            if (a.AttributeClass is null)
                return false;
            var attributeFullName = a.AttributeClass.GetTypeSymbolFullName(withGlobalPrefix: false);
            return attributeFullName == "Mediator.MediatorOptionsAttribute" || attributeFullName == "MediatorOptions";
        });
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
                ReportDiagnostic(
                    lifetimeArgument.Expression.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(l, "Expected lambda expression")
                );
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
                if (
                    parameterTypeName != ""
                    && parameterTypeName != "Mediator.MediatorOptions"
                    // TODO: now that options are generated in the same source gen step, full name gets resolved to this, this is hopefully temporary
                    && parameterTypeName != "MediatorOptions"
                )
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
                        ReportDiagnostic(
                            statement.GetLocation(),
                            (in CompilationAnalyzerContext c, Location l) =>
                                c.ReportInvalidCodeBasedConfiguration(l, "Expected statement expression")
                        );
                        continue;
                    }
                    if (statementExpression.Expression is not AssignmentExpressionSyntax assignment)
                    {
                        ReportDiagnostic(
                            statementExpression.Expression.GetLocation(),
                            (in CompilationAnalyzerContext c, Location l) =>
                                c.ReportInvalidCodeBasedConfiguration(l, "Expected assignment expression")
                        );
                        continue;
                    }
                    if (!ProcessAddMediatorAssignmentStatement(assignment, semanticModel, cancellationToken))
                        continue;
                }
            }
            else
            {
                ReportDiagnostic(
                    body.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(l, "Expected block or simple assignment")
                );
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

        var syntaxReference = optionsAttr.ApplicationSyntaxReference;
        var optionsAttrSyntax = optionsAttr.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;
        if (syntaxReference is null || optionsAttrSyntax is null || optionsAttrSyntax.ArgumentList is null)
            return;

        if (configuredByAddMediator)
        {
            ReportDiagnostic(
                optionsAttrSyntax.GetLocation(),
                (in CompilationAnalyzerContext c, Location l) => c.ReportConflictingConfiguration(l)
            );
        }

        var semanticModel = compilation.GetSemanticModel(syntaxReference.SyntaxTree);

        foreach (var attrArg in optionsAttrSyntax.ArgumentList.Arguments)
        {
            if (attrArg.NameEquals is null)
                throw new Exception("Error parsing MediatorOptions");

            var attrFieldName = attrArg.NameEquals.Name.ToString();
            if (attrFieldName == "ServiceLifetime")
            {
                var identifierNameSyntax = (IdentifierNameSyntax)
                    ((MemberAccessExpressionSyntax)attrArg.Expression).Name;
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
            else if (attrFieldName == "NotificationPublisherType")
            {
                if (attrArg.Expression is not TypeOfExpressionSyntax identifier)
                {
                    ReportDiagnostic(
                        attrArg.Expression.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                "Must provide a typeof expression to 'NotificationPublisherType' configuration"
                            )
                    );
                    return;
                }
                var typeSymbol = semanticModel.GetTypeInfo(identifier.Type, cancellationToken).Type;
                if (typeSymbol is null)
                {
                    ReportDiagnostic(
                        identifier.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(l, $"Could not resolve type: {identifier.Type}")
                    );
                    return;
                }

                if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                {
                    ReportDiagnostic(
                        identifier.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                "Type configuration for NotificationPublisherType must be a named type"
                            )
                    );
                    return;
                }

                if (!Context.Compilation.HasImplicitConversion(namedTypeSymbol, _notificationPublisherInterfaceSymbol))
                {
                    ReportDiagnostic(
                        identifier.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                "The type provided for NotificationPublisherType must implement the INotificationPublisher interface"
                            )
                    );
                    return;
                }

                _notificationPublisherImplementationSymbol = namedTypeSymbol;
            }
            else if (attrFieldName == "CachingMode")
            {
                // Handle CachingMode configuration from attribute
                // Since this enum is generated by this source generator, we can't always rely on symbol resolution
                // Instead, we parse the syntax directly to get the enum value name
                string? cachingModeName = null;

                if (attrArg.Expression is MemberAccessExpressionSyntax enumAccess)
                {
                    var identifierNameSyntax = (IdentifierNameSyntax)enumAccess.Name;
                    cachingModeName = identifierNameSyntax.Identifier.Text;

                    // Try to resolve symbol if available
                    if (_cachingModeEnumSymbol != null)
                    {
                        _configuredCachingModeSymbol = (IFieldSymbol?)
                            _cachingModeEnumSymbol
                                .GetMembers()
                                .OfType<IFieldSymbol>()
                                .FirstOrDefault(m => m.Name == cachingModeName);
                    }
                    else if (cachingModeName == "Lazy")
                    {
                        // Symbols not loaded yet, but we parsed "Lazy" from syntax
                        // Mark it for lazy mode (will use name-based check in properties)
                        _configuredLazyCachingModeFromSyntax = true;
                    }
                }
                else
                {
                    ReportDiagnostic(
                        attrArg.Expression.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(l, "Could not resolve CachingMode configuration")
                    );
                    return;
                }
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
        Debug.Assert(_pipelineBehaviorInterfaceSymbol is not null);
        Debug.Assert(_streamPipelineBehaviorInterfaceSymbol is not null);
        Debug.Assert(_unitSymbol is not null);

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
                    ReportDiagnostic(
                        assignment.Right.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(l, "Could not resolve namespace configuration")
                    );
                    return false;
                }
            }
            else
            {
                ReportDiagnostic(
                    assignment.Right.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(
                            l,
                            "Expected literal or identifier in namespace configuration"
                        )
                );
                return false;
            }
        }
        else if (opt == "GenerateTypesAsInternal")
        {
            if (assignment.Right is not LiteralExpressionSyntax literal)
            {
                ReportDiagnostic(
                    assignment.Right.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(
                            l,
                            "Expected literal expression for 'GenerateTypesAsInternal'"
                        )
                );
                return false;
            }

            if (literal.IsKind(SyntaxKind.TrueLiteralExpression))
                GenerateTypesAsInternal = true;
            else if (literal.IsKind(SyntaxKind.FalseLiteralExpression))
                GenerateTypesAsInternal = false;
            else
            {
                ReportDiagnostic(
                    assignment.Right.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(
                            l,
                            "Expected boolean literal expression for 'GenerateTypesAsInternal'"
                        )
                );
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
                ReportDiagnostic(
                    assignment.Right.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(l, "Could not resolve lifetime configuration")
                );
                return false;
            }
        }
        else if (opt == "NotificationPublisherType")
        {
            if (assignment.Right is TypeOfExpressionSyntax identifier)
            {
                var typeSymbol = semanticModel.GetTypeInfo(identifier.Type, cancellationToken).Type;
                if (typeSymbol is null)
                {
                    ReportDiagnostic(
                        identifier.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                $"Could not resolve type for NotificationPublisherType: {identifier.Type}"
                            )
                    );
                    return false;
                }

                if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                {
                    ReportDiagnostic(
                        identifier.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                "Type configuration for NotificationPublisherType must be a named type"
                            )
                    );
                    return false;
                }

                // Check if namedTypeSymbol is assignable to handler
                if (!Context.Compilation.HasImplicitConversion(namedTypeSymbol, _notificationPublisherInterfaceSymbol))
                {
                    ReportDiagnostic(
                        identifier.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                "The type provided for NotificationPublisherType must implement the INotificationPublisher interface"
                            )
                    );
                    return false;
                }

                _notificationPublisherImplementationSymbol = namedTypeSymbol;
            }
            else
            {
                ReportDiagnostic(
                    assignment.Right.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(
                            l,
                            "NotificationPublisherType must be configured with a typeof expression"
                        )
                );
                return false;
            }
        }
        else if (opt == "CachingMode")
        {
            // Handle CachingMode configuration
            // Since this enum is generated by this source generator, we can't always rely on symbol resolution
            // Instead, we parse the syntax directly to get the enum value name
            string? cachingModeName = null;

            if (assignment.Right is MemberAccessExpressionSyntax enumAccess)
            {
                var identifierNameSyntax = (IdentifierNameSyntax)enumAccess.Name;
                cachingModeName = identifierNameSyntax.Identifier.Text;

                // Try to resolve symbol if available
                if (_cachingModeEnumSymbol != null)
                {
                    _configuredCachingModeSymbol = (IFieldSymbol?)
                        _cachingModeEnumSymbol
                            .GetMembers()
                            .OfType<IFieldSymbol>()
                            .FirstOrDefault(m => m.Name == cachingModeName);
                }
                else if (cachingModeName == "Lazy")
                {
                    // Symbols not loaded yet, but we parsed "Lazy" from syntax
                    // Mark it for lazy mode (will use name-based check in properties)
                    _configuredLazyCachingModeFromSyntax = true;
                }
            }
            else if (assignment.Right is IdentifierNameSyntax identifier)
            {
                _configuredCachingModeSymbol = GetCachingModeSymbol(identifier, semanticModel, cancellationToken);
            }
            else
            {
                ReportDiagnostic(
                    assignment.Right.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(l, "Could not resolve CachingMode configuration")
                );
                return false;
            }
        }
        else if (opt == "Assemblies")
        {
            if (_configuredAssemblies is not null)
            {
                ReportDiagnostic(
                    assignment.Left.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(l, "Assemblies can only be configured once")
                );
                return false;
            }

            var assemblyCache = new Dictionary<IModuleSymbol, ImmutableArray<IAssemblySymbol>>(
                SymbolEqualityComparer.Default
            );
            _configuredAssemblies = new();
            var typeOfExpressions = assignment.Right.DescendantNodes().OfType<TypeOfExpressionSyntax>().ToArray();
            HashSet<IAssemblySymbol> visitedAssemblies = new(_symbolComparer);
            foreach (var typeOfExpression in typeOfExpressions)
            {
                var typeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type, cancellationToken);
                if (typeInfo.Type is not INamedTypeSymbol typeSymbol)
                {
                    ReportDiagnostic(
                        typeOfExpression.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(l, $"Could not resolve type: {typeOfExpression.Type}")
                    );
                    continue;
                }
                var assemblySymbol = typeSymbol.OriginalDefinition.ContainingAssembly;

                if (_symbolComparer.Equals(_unitSymbol!.ContainingAssembly, assemblySymbol))
                {
                    ReportDiagnostic(
                        typeOfExpression.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                "Tried to configure 'Mediator.Abstractions' as an assembly"
                            )
                    );
                    continue;
                }

                if (!visitedAssemblies.Add(assemblySymbol))
                {
                    ReportDiagnostic(
                        typeOfExpression.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                $"The assembly '{assemblySymbol.Name}' is already configured"
                            )
                    );
                    continue;
                }

                if (!assemblySymbol.Modules.Any(m => IsMediatorLibReferencedByTheModule(m, assemblyCache)))
                {
                    ReportDiagnostic(
                        typeOfExpression.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                "Configured assembly does not reference 'Mediator.Abstractions', so it cannot have messages and handlers"
                            )
                    );
                    continue;
                }

                _configuredAssemblies.Enqueue(assemblySymbol.GlobalNamespace);
            }
        }
        else if (opt == "PipelineBehaviors" || opt == "StreamPipelineBehaviors")
        {
            var interfaceName = opt == "PipelineBehaviors" ? "IPipelineBehavior" : "IStreamPipelineBehavior";
            var typeOfExpressions = assignment.Right.DescendantNodes().OfType<TypeOfExpressionSyntax>().ToArray();

            HashSet<INamedTypeSymbol> pipelineBehaviorTypeSymbols = new(_symbolComparer);
            foreach (var typeOfExpression in typeOfExpressions)
            {
                var typeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type, cancellationToken);
                if (typeInfo.Type is not INamedTypeSymbol pipelineTypeSymbol)
                {
                    ReportDiagnostic(
                        typeOfExpression.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(l, $"Could not resolve type: {typeOfExpression.Type}")
                    );
                    continue;
                }
                pipelineTypeSymbol = pipelineTypeSymbol.OriginalDefinition;

                var interfaceSymbol =
                    opt == "PipelineBehaviors"
                        ? _pipelineBehaviorInterfaceSymbol!
                        : _streamPipelineBehaviorInterfaceSymbol!;
                if (
                    !pipelineTypeSymbol.AllInterfaces.Any(i =>
                        i.IsGenericType && i.OriginalDefinition.Equals(interfaceSymbol, _symbolComparer)
                    )
                )
                {
                    ReportDiagnostic(
                        typeOfExpression.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                $"The type '{typeOfExpression.Type}' does not implement '{interfaceName}'"
                            )
                    );
                    continue;
                }

                if (!pipelineBehaviorTypeSymbols.Add(pipelineTypeSymbol))
                {
                    ReportDiagnostic(
                        typeOfExpression.Type.GetLocation(),
                        (in CompilationAnalyzerContext c, Location l) =>
                            c.ReportInvalidCodeBasedConfiguration(
                                l,
                                $"The type '{typeOfExpression.Type}' is duplicated in the pipeline configuration"
                            )
                    );
                    continue;
                }

                var pipelineBehaviorType = new PipelineBehaviorType(pipelineTypeSymbol, interfaceSymbol, this);
                _pipelineBehaviors.Add(pipelineBehaviorType);
            }
        }
        else
        {
            ReportDiagnostic(
                assignment.Left.GetLocation(),
                (in CompilationAnalyzerContext c, Location l) =>
                    c.ReportInvalidCodeBasedConfiguration(l, $"Unrecognized option: {opt}")
            );
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
                IPropertySymbol propertySymbol => TryResolveNamespaceSymbol(
                    propertySymbol,
                    semanticModel,
                    cancellationToken
                ),
                ILocalSymbol localSymbol => TryResolveNamespaceSymbol(localSymbol, semanticModel, cancellationToken),
                _ => null,
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

            var initializerExpression =
                syntaxNode is VariableDeclaratorSyntax variableDeclarator ? variableDeclarator.Initializer?.Value
                : syntaxNode is PropertyDeclarationSyntax propertyDeclaration ? propertyDeclaration.Initializer?.Value
                : null;

            if (initializerExpression is null)
            {
                ReportDiagnostic(
                    syntaxNode.GetLocation(),
                    (in CompilationAnalyzerContext c, Location l) =>
                        c.ReportInvalidCodeBasedConfiguration(l, "Failed to resolve namespace configuration")
                );
                return null;
            }

            if (initializerExpression is LiteralExpressionSyntax literal)
                return literal.Token.ValueText;

            if (initializerExpression is IdentifierNameSyntax reference)
                return TryResolveNamespaceIdentifier(reference, semanticModel, cancellationToken);

            return null;
        }
    }

    private IFieldSymbol? GetEnumSymbol(
        IdentifierNameSyntax identifierNameSyntax,
        SemanticModel semanticModel,
        INamedTypeSymbol? enumSymbol,
        Func<ISymbol, SemanticModel, CancellationToken, IFieldSymbol?> tryResolver,
        CancellationToken cancellationToken
    )
    {
        var symbol = semanticModel.GetSymbolInfo(identifierNameSyntax, cancellationToken).Symbol;
        if (
            symbol is IFieldSymbol fieldSymbol
            && enumSymbol is not null
            && SymbolEqualityComparer.Default.Equals(enumSymbol, fieldSymbol.ContainingType)
        )
            return fieldSymbol;

        return symbol switch
        {
            IFieldSymbol fs => tryResolver(fs, semanticModel, cancellationToken),
            IPropertySymbol ps => tryResolver(ps, semanticModel, cancellationToken),
            ILocalSymbol ls => tryResolver(ls, semanticModel, cancellationToken),
            _ => null,
        };
    }

    private IFieldSymbol? TryGetEnumSymbol(
        ISymbol symbol,
        SemanticModel semanticModel,
        INamedTypeSymbol? enumSymbol,
        string configName,
        Func<IdentifierNameSyntax, SemanticModel, CancellationToken, IFieldSymbol?> getResolver,
        CancellationToken cancellationToken
    )
    {
        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.HasConstantValue)
        {
            var value = (int)fieldSymbol.ConstantValue!;
            return enumSymbol?.GetMembers().OfType<IFieldSymbol>().Single(m => (int)m.ConstantValue! == value);
        }
        if (symbol is ILocalSymbol localSymbol && localSymbol.HasConstantValue)
        {
            var value = (int)localSymbol.ConstantValue!;
            return enumSymbol?.GetMembers().OfType<IFieldSymbol>().Single(m => (int)m.ConstantValue! == value);
        }

        var syntaxReferences = symbol.DeclaringSyntaxReferences;
        var syntaxNode = syntaxReferences.First().GetSyntax(cancellationToken);

        var initializerExpression =
            syntaxNode is VariableDeclaratorSyntax variableDeclarator ? variableDeclarator.Initializer?.Value
            : syntaxNode is PropertyDeclarationSyntax propertyDeclaration ? propertyDeclaration.Initializer?.Value
            : null;

        if (initializerExpression is null)
        {
            ReportDiagnostic(
                syntaxNode.GetLocation(),
                (in CompilationAnalyzerContext c, Location l) =>
                    c.ReportInvalidCodeBasedConfiguration(l, $"Failed to resolve {configName} configuration")
            );
            return null;
        }

        if (initializerExpression is LiteralExpressionSyntax literal)
        {
            var value = (int)literal.Token.Value!;
            return enumSymbol?.GetMembers().OfType<IFieldSymbol>().Single(m => (int)m.ConstantValue! == value);
        }

        if (initializerExpression is MemberAccessExpressionSyntax memberAccess)
            return getResolver((IdentifierNameSyntax)memberAccess.Name, semanticModel, cancellationToken);

        if (initializerExpression is IdentifierNameSyntax reference)
            return getResolver(reference, semanticModel, cancellationToken);

        return null;
    }

    private IFieldSymbol? GetServiceLifetimeSymbol(
        IdentifierNameSyntax identifierNameSyntax,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        return GetEnumSymbol(
            identifierNameSyntax,
            semanticModel,
            _serviceLifetimeEnumSymbol,
            TryGetServiceLifetimeSymbol,
            cancellationToken
        );
    }

    private IFieldSymbol? TryGetServiceLifetimeSymbol(
        ISymbol symbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        return TryGetEnumSymbol(
            symbol,
            semanticModel,
            _serviceLifetimeEnumSymbol,
            "lifetime",
            GetServiceLifetimeSymbol,
            cancellationToken
        );
    }

    private IFieldSymbol? GetCachingModeSymbol(
        IdentifierNameSyntax identifierNameSyntax,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        return GetEnumSymbol(
            identifierNameSyntax,
            semanticModel,
            _cachingModeEnumSymbol,
            TryGetCachingModeSymbol,
            cancellationToken
        );
    }

    private IFieldSymbol? TryGetCachingModeSymbol(
        ISymbol symbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        return TryGetEnumSymbol(
            symbol,
            semanticModel,
            _cachingModeEnumSymbol,
            "CachingMode",
            GetCachingModeSymbol,
            cancellationToken
        );
    }

    private delegate Diagnostic ReportDiagnosticDelegate<T>(in CompilationAnalyzerContext context, T state);

    private void ReportDiagnostic(Diagnostic diagnostic)
    {
        _context.ReportDiagnostic(diagnostic);
    }

    private void ReportDiagnostic<T>(T state, ReportDiagnosticDelegate<T> del)
    {
        var diagnostic = del(in _context, state);
        _hasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
    }

    private delegate Diagnostic ReportDiagnosticDelegate(in CompilationAnalyzerContext context);

    private void ReportDiagnostic(ReportDiagnosticDelegate del)
    {
        var diagnostic = del(in _context);
        _hasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
    }
}
