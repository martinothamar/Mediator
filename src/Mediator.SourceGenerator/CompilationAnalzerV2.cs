using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator
{
    internal abstract class SymbolMetadata<T> : IEquatable<T?>
        where T : SymbolMetadata<T>
    {
        private static readonly SymbolEqualityComparer _comparer = SymbolEqualityComparer.Default;
        public readonly INamedTypeSymbol Symbol;
        protected readonly CompilationAnalyzerV2 Analyzer;

        public SymbolMetadata(INamedTypeSymbol symbol, CompilationAnalyzerV2 analyzer)
        {
            Symbol = symbol;
            Analyzer = analyzer;
        }

        public override bool Equals(object? obj) => Equals(obj as T);

        public bool Equals(T? other) => other != null && _comparer.Equals(Symbol, other.Symbol);

        public override int GetHashCode() => _comparer.GetHashCode(Symbol);

        public override string ToString() => Symbol.Name;

        public bool IsStruct => Symbol.TypeKind == TypeKind.Struct;
        public bool IsReadOnly => Symbol.IsReadOnly;
        public string ParameterModifier => IsStruct && IsReadOnly ? "in " : string.Empty;
    }

    internal sealed class RequestMessage : SymbolMetadata<RequestMessage>
    {
        public RequestMessageHandler? Handler { get; private set; }

        public readonly INamedTypeSymbol ResponseSymbol;

        public readonly RequestMessageHandlerWrapper WrapperType;

        public readonly string MessageType;

        public RequestMessage(INamedTypeSymbol symbol, INamedTypeSymbol responseSymbol, string messageType, CompilationAnalyzerV2 analyzer)
            : base(symbol, analyzer)
        {
            ResponseSymbol = responseSymbol;
            WrapperType = analyzer.RequestMessageHandlerWrappers.Single(w => w.MessageType == messageType);
            MessageType = messageType;
        }

        public string RequestFullName => Symbol.GetTypeSymbolFullName();
        public string ResponseFullName => ResponseSymbol!.GetTypeSymbolFullName();

        public void SetHandler(RequestMessageHandler handler) => Handler = handler;

        public string HandlerWrapperTypeNameWithGenericTypeArguments =>
            WrapperType.HandlerWrapperTypeNameWithGenericTypeArguments(Symbol, ResponseSymbol);

        public string PipelineHandlerType =>
            $"global::Mediator.IPipelineBehavior<{RequestFullName}, {ResponseFullName}>";


        public string HandlerWrapperPropertyName =>
           $"Wrapper_For_{Symbol.GetTypeSymbolFullName(withGlobalPrefix: false, includeTypeParameters: false).Replace("global::", "").Replace('.', '_')}";

        public string SyncMethodName => "Send";
        public string AsyncMethodName => "Send";

        public string SyncReturnType => ResponseSymbol.GetTypeSymbolFullName();
        public string AsyncReturnType => Analyzer
            .Compilation
            .GetTypeByMetadataName(typeof(ValueTask<>).FullName!)!
            .Construct(ResponseSymbol)
            .GetTypeSymbolFullName();
    }

    internal sealed class NotificationMessage : SymbolMetadata<NotificationMessage>
    {
        private readonly HashSet<NotificationMessageHandler> _handlers;

        public NotificationMessage(INamedTypeSymbol symbol, CompilationAnalyzerV2 analyzer)
            : base(symbol, analyzer)
        {
            _handlers = new ();
        }

        internal void AddHandlers(NotificationMessageHandler handler) => _handlers.Add(handler);

        public string ServiceLifetime => "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton";

        public string HandlerTypeOfExpression => $"typeof(global::Mediator.INotificationHandler<{Symbol.GetTypeSymbolFullName()}>)";

        public IEnumerable<string> HandlerServicesRegistrationBlock
        {
            get
            {
                foreach (var handler in _handlers)
                {
                    var getExpression = $"sp => sp.GetRequiredService<{handler.FullName}>()";
                    yield return $"services.Add(new SD({HandlerTypeOfExpression}, {getExpression}, {ServiceLifetime}));";
                }
            }
        }
    }

    internal abstract class MessageHandler<T> : SymbolMetadata<MessageHandler<T>>
    {
        public MessageHandler(INamedTypeSymbol symbol, CompilationAnalyzerV2 analyzer)
            : base(symbol, analyzer)
        {
        }

        public bool IsOpenGeneric => Symbol.TypeArguments.Length > 0;

        public string FullName => Symbol.GetTypeSymbolFullName();

        public string TypeOfExpression(bool includeTypeParameters = true)
        {
            var typeName = Symbol.GetTypeSymbolFullName(includeTypeParameters: includeTypeParameters);
            var genericArguments = string.Empty;
            if (IsOpenGeneric && !includeTypeParameters)
                genericArguments = $"<{new string(',', Symbol.TypeArguments.Length - 1)}>";
            return $"typeof({typeName}{genericArguments})";
        }

        public string ServiceRegistrationBlock =>
            $"services.TryAdd(new SD({TypeOfExpression()}, {TypeOfExpression()}, {ServiceLifetime}));";

        public string ServiceLifetime =>
            "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton";
    }

    internal sealed class RequestMessageHandler : MessageHandler<RequestMessageHandler>
    {
        public readonly string MessageType;
        public readonly RequestMessageHandlerWrapper WrapperType;

        public RequestMessageHandler(INamedTypeSymbol symbol, string messageType, CompilationAnalyzerV2 analyzer)
            : base(symbol, analyzer)
        {
            MessageType = messageType;
            WrapperType = analyzer.RequestMessageHandlerWrappers.Single(w => w.MessageType == messageType);
        }
    }

    internal sealed class RequestMessageHandlerWrapper
    {
        public readonly string MessageType;
        private readonly CompilationAnalyzerV2 _analyzer;

        public RequestMessageHandlerWrapper(string messageType, CompilationAnalyzerV2 analyzer)
        {
            MessageType = messageType;
            _analyzer = analyzer;
        }

        public string FullNamespace =>
            $"global::{_analyzer.MediatorNamespace}";

        public string HandlerWrapperTypeName(TypeKind type) =>
            $"{MessageType}{(type == TypeKind.Struct ? "Struct" : "Class")}HandlerWrapper";

        public string HandlerWrapperTypeFullName(TypeKind type) =>
            $"{FullNamespace}.{HandlerWrapperTypeName(type)}";

        public string HandlerWrapperTypeNameWithGenericTypeArguments(TypeKind type) =>
            $"{HandlerWrapperTypeName(type)}<TRequest, TResponse>";

        public string HandlerWrapperTypeNameWithGenericTypeArguments(INamedTypeSymbol requestSymbol, INamedTypeSymbol responseSymbol) =>
            $"{HandlerWrapperTypeFullName(requestSymbol.TypeKind)}<{requestSymbol.GetTypeSymbolFullName()}, {responseSymbol.GetTypeSymbolFullName()}>";

        public string HandlerWrapperTypeOfExpression(TypeKind type) =>
            $"typeof({HandlerWrapperTypeFullName(type)}<,>)";

        public string ClassHandlerWrapperTypeNameWithGenericTypeArguments =>
            HandlerWrapperTypeNameWithGenericTypeArguments(TypeKind.Class);

        public string StructHandlerWrapperTypeNameWithGenericTypeArguments =>
            HandlerWrapperTypeNameWithGenericTypeArguments(TypeKind.Struct);

        public string ClassHandlerWrapperTypeName =>
            HandlerWrapperTypeName(TypeKind.Class);

        public string StructHandlerWrapperTypeName =>
            HandlerWrapperTypeName(TypeKind.Struct);
    }

    internal sealed class NotificationMessageHandler : MessageHandler<NotificationMessageHandler>
    {
        public NotificationMessageHandler(INamedTypeSymbol symbol, CompilationAnalyzerV2 analyzer)
            : base(symbol, analyzer)
        {
        }

        public string OpenGenericTypeOfExpression =>
            $"typeof(global::Mediator.INotificationHandler<>)";

        public string OpenGenericServiceRegistrationBlock =>
            $"services.Add(new SD({OpenGenericTypeOfExpression}, {TypeOfExpression(false)}, {ServiceLifetime}));";
    }

    internal sealed class CompilationAnalyzerV2
    {
        private static readonly SymbolEqualityComparer _symbolComparer = SymbolEqualityComparer.Default;
        private readonly GeneratorExecutionContext _context;
        private readonly Compilation _compilation;
        private readonly HashSet<RequestMessage> _requestMessages;
        private readonly HashSet<NotificationMessage> _notificationMessages;
        private readonly HashSet<RequestMessageHandler> _requestMessageHandlers;
        private readonly HashSet<NotificationMessageHandler> _notificationMessageHandlers;

        public readonly ImmutableArray<RequestMessageHandlerWrapper> RequestMessageHandlerWrappers;

        private readonly INamedTypeSymbol[] _baseHandlerSymbols;
        private readonly INamedTypeSymbol[] _baseMessageSymbols;
        private readonly HashSet<INamedTypeSymbol> _baseHandlerSymbolsSet;
        private readonly HashSet<INamedTypeSymbol> _baseMessageSymbolsSet;

        private readonly INamedTypeSymbol _notificationHandlerInterfaceSymbol;
        private readonly INamedTypeSymbol _notificationInterfaceSymbol;

        public IEnumerable<RequestMessage> RequestMessages =>
            _requestMessages.Where(r => r.Handler is not null);

        public IEnumerable<NotificationMessage> NotificationMessages =>
            _notificationMessages;

        public IEnumerable<RequestMessageHandler> RequestMessageHandlers =>
            _requestMessageHandlers;

        public IEnumerable<NotificationMessageHandler> NotificationMessageHandlers =>
            _notificationMessageHandlers.Where(h => !h.IsOpenGeneric);

        public IEnumerable<NotificationMessageHandler> OpenGenericNotificationMessageHandlers =>
            _notificationMessageHandlers.Where(h => h.IsOpenGeneric);

        public bool HasRequests => _requestMessages.Any(r => r.MessageType == "Request");
        public bool HasCommands => _requestMessages.Any(r => r.MessageType == "Command");
        public bool HasQueries => _requestMessages.Any(r => r.MessageType == "Query");

        public IEnumerable<RequestMessage> IRequestMessages => _requestMessages.Where(r => r.Handler is not null && r.MessageType == "Request");
        public IEnumerable<RequestMessage> ICommandMessages => _requestMessages.Where(r => r.Handler is not null && r.MessageType == "Command");
        public IEnumerable<RequestMessage> IQueryMessages => _requestMessages.Where(r => r.Handler is not null && r.MessageType == "Query");

        private bool _hasErrors;

        public bool HasErrors => _hasErrors;

        public INamedTypeSymbol UnitSymbol { get; }

        public Compilation Compilation => _compilation;

        public string MediatorNamespace { get; private set; } = Constants.MediatorLib;

        public CompilationAnalyzerV2(in GeneratorExecutionContext context)
        {
            _context = context;
            _compilation = context.Compilation;

            _requestMessages = new ();
            _notificationMessages = new ();
            _requestMessageHandlers = new ();
            _notificationMessageHandlers = new ();

            UnitSymbol = _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.Unit")!.OriginalDefinition;

            _baseHandlerSymbols = new INamedTypeSymbol[]
            {
                // Handlers
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequestHandler`1")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequestHandler`2")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommandHandler`1")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommandHandler`2")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IQueryHandler`2")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.INotificationHandler`1")!.OriginalDefinition,
            };
            _baseHandlerSymbolsSet = new(_baseHandlerSymbols, _symbolComparer);

            RequestMessageHandlerWrappers = new RequestMessageHandlerWrapper[]
            {
                new RequestMessageHandlerWrapper("Request", this),
                new RequestMessageHandlerWrapper("Command", this),
                new RequestMessageHandlerWrapper("Query", this),
            }.ToImmutableArray();

            _notificationHandlerInterfaceSymbol = _baseHandlerSymbols[_baseHandlerSymbols.Length - 1];

            _baseMessageSymbols = new INamedTypeSymbol[]
            {
                // Message types
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequest")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IRequest`1")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommand")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.ICommand`1")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IQuery`1")!.OriginalDefinition,
                _compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.INotification")!.OriginalDefinition,
            };
            _baseMessageSymbolsSet = new (_baseMessageSymbols, _symbolComparer);

            _notificationInterfaceSymbol = _baseMessageSymbols[_baseMessageSymbols.Length - 1];
        }

        public void Analyze(CancellationToken cancellationToken)
        {
            TryParseOptions(cancellationToken);

            var queue = new Queue<INamespaceOrTypeSymbol>();

            FindGlobalNamespaces(queue);

            PopulateMetadata(queue);

            //ProcessOpenGenericHandlers();
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

        private void PopulateMetadata(Queue<INamespaceOrTypeSymbol> queue)
        {
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

            foreach (var notificationMessage in _notificationMessages)
            {
                foreach (var notificationMessageHandler in _notificationMessageHandlers)
                {
                    if (notificationMessageHandler.IsOpenGeneric) // These are added as open generics
                        continue;

                    foreach (var notificationMessageHandlerInterfaceSymbol in notificationMessageHandler.Symbol.AllInterfaces)
                    {
                        if (notificationMessageHandlerInterfaceSymbol.ContainingNamespace.Name != Constants.MediatorLib)
                            continue;

                        if (!_symbolComparer.Equals(notificationMessageHandlerInterfaceSymbol.OriginalDefinition, _notificationHandlerInterfaceSymbol))
                            continue;

                        var candidateNotificationMessageSymbol = (INamedTypeSymbol)notificationMessageHandlerInterfaceSymbol.TypeArguments[0];

                        if (_symbolComparer.Equals(candidateNotificationMessageSymbol, notificationMessage.Symbol))
                            notificationMessage.AddHandlers(notificationMessageHandler);
                        else if (_compilation.HasImplicitConversion(notificationMessage.Symbol, candidateNotificationMessageSymbol))
                            notificationMessage.AddHandlers(notificationMessageHandler);
                    }
                }
            }

            ;

            const int NOT_RELEVANT = 0;
            const int IS_REQUEST_HANDLER = 1;
            const int IS_NOTIFICATION_HANDLER = 2;
            const int IS_REQUEST = 3;
            const int IS_NOTIFICATION = 4;

            void ProcessMember(Queue<INamespaceOrTypeSymbol> queue, INamespaceOrTypeSymbol member, Dictionary<INamedTypeSymbol, object?> mapping)
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

                for (int i = 0; i < typeSymbol.AllInterfaces.Length; i++)
                {
                    var typeInterfaceSymbol = typeSymbol.AllInterfaces[i];

                    if (typeInterfaceSymbol.ContainingNamespace.Name != Constants.MediatorLib)
                        continue;

                    if (!ProcessInterface(i, typeSymbol, typeInterfaceSymbol, isStruct, mapping))
                        break;
                }
            }

            bool ProcessInterface(int i, INamedTypeSymbol typeSymbol, INamedTypeSymbol typeInterfaceSymbol, bool isStruct, Dictionary<INamedTypeSymbol, object?> mapping)
            {
                var codeOfInterest = IsInteresting(typeInterfaceSymbol);
                switch (codeOfInterest)
                {
                    case NOT_RELEVANT: break; // Continue loop
                    case IS_REQUEST_HANDLER:
                    case IS_NOTIFICATION_HANDLER:
                        {
                            if (isStruct)
                            {
                                // Handlers must be classes
                                ReportDiagnostic(typeSymbol, (in GeneratorExecutionContext c, INamedTypeSymbol s) => c.ReportInvalidHandlerType(s));
                                return false;
                            }

                            // TODO, return here? AllInterfaces ordered?
                            if (IsAlreadyHandledByDerivedInterface(i, 1, typeSymbol, typeInterfaceSymbol))
                                break;

                            var isRequest = codeOfInterest == IS_REQUEST_HANDLER;

                            if (isRequest)
                            {
                                if (IsOpenGeneric(typeSymbol))
                                {
                                    // Handlers must be classes
                                    ReportDiagnostic(typeSymbol, (in GeneratorExecutionContext c, INamedTypeSymbol s) => c.ReportOpenGenericRequestHandler(s));
                                    return false;
                                }

                                var messageType = typeInterfaceSymbol.Name.Substring(1, typeInterfaceSymbol.Name.IndexOf("Handler") - 1);

                                var handler = new RequestMessageHandler(typeSymbol, messageType, this);
                                var requestMessageSymbol = (INamedTypeSymbol)typeInterfaceSymbol.TypeArguments[0];
                                if (mapping.TryGetValue(requestMessageSymbol, out var requestMessageObj))
                                {
                                    if (requestMessageObj is null || requestMessageObj is not RequestMessage requestMessage)
                                    {
                                        // Signal that we have duplicates
                                        ReportDiagnostic(typeSymbol, (in GeneratorExecutionContext c, INamedTypeSymbol s) => c.ReportMultipleHandlers(s));
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
                            var responseMessageSymbol = typeInterfaceSymbol.TypeArguments.Length > 0 ?
                                (INamedTypeSymbol)typeInterfaceSymbol.TypeArguments[0] :
                                UnitSymbol;

                            if (IsAlreadyHandledByDerivedInterface(i, 0, typeSymbol, typeInterfaceSymbol))
                                break;

                            var messageType = typeInterfaceSymbol.Name.IndexOf('<') == -1 ?
                                typeInterfaceSymbol.Name.Substring(1) :
                                typeInterfaceSymbol.Name.Substring(1, typeInterfaceSymbol.Name.IndexOf('<') - 1);

                            var message = new RequestMessage(typeSymbol, responseMessageSymbol, messageType, this);
                            if (!_requestMessages.Add(message))
                            {
                                // If this symbol has already been added,
                                // the type implements multiple base message types.
                                ReportDiagnostic(typeSymbol, (in GeneratorExecutionContext c, INamedTypeSymbol s) => c.ReportMessageDerivesFromMultipleMessageInterfaces(s));
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
                                ReportDiagnostic(typeSymbol, (in GeneratorExecutionContext c, INamedTypeSymbol s) => c.ReportMessageDerivesFromMultipleMessageInterfaces(s));
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
                        return _symbolComparer.Equals(baseSymbol, _notificationHandlerInterfaceSymbol) ?
                            IS_NOTIFICATION_HANDLER :
                            IS_REQUEST_HANDLER;
                }

                for (int i = 0; i < _baseMessageSymbols.Length; i++)
                {
                    var baseSymbol = _baseMessageSymbols[i];
                    if (_symbolComparer.Equals(baseSymbol, originalInterfaceSymbol))
                        return _symbolComparer.Equals(baseSymbol, _notificationInterfaceSymbol) ?
                            IS_NOTIFICATION :
                            IS_REQUEST;
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
                var mightSkip = typeInterfaceSymbol.TypeArguments.Length > responseTypeArgumentIndex &&
                    typeInterfaceSymbol.TypeArguments[responseTypeArgumentIndex] is INamedTypeSymbol responseTypeSymbol &&
                    _symbolComparer.Equals(responseTypeSymbol, UnitSymbol);

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

        //private void ProcessOpenGenericHandlers()
        //{
        //    var requestMessageHandlers = _requestMessageHandlers.ToArray();

        //    for (int i = 0; i < requestMessageHandlers.Length; i++)
        //    {
        //        var requestMessageHandler = requestMessageHandlers[i];

        //        if (IsOpenGeneric(requestMessageHandler.Symbol))
        //        {
        //            var requestHandlerRequestTypeParameter = (ITypeParameterSymbol)requestMessageHandler.Symbol.TypeArguments[0];

        //            if (requestHandlerRequestTypeParameter.ConstraintTypes.Length == 0)
        //                throw new Exception("TODO report diag - unconstrained generic handler type");

        //            var constraint = requestHandlerRequestTypeParameter.ConstraintTypes[0];
        //            // TODO what2do with several constraints?

        //            foreach (var requestMessage in _requestMessages)
        //            {
        //                if (_symbolComparer.Equals(requestMessage.Symbol, constraint))
        //                    continue;

        //                if (!_compilation.ClassifyConversion(requestMessage.Symbol, constraint).IsImplicit)
        //                    continue;

        //                var constructedRequestMessageHandlerSymbol = requestMessageHandler.Symbol.Construct(requestMessage.Symbol);
        //                var requestMessageHandlerInterfaceSymbol = GetHandlerInterfaceForRequest(requestMessage.Symbol);
        //                // var constructedRequestMessageHandlerInterfaceSymbol = 
        //            }
        //        }
        //    }

        //    INamedTypeSymbol GetHandlerInterfaceForRequest(INamedTypeSymbol requestMessageSymbol)
        //    {
        //        foreach (var baseRequestMessage in _baseMessageSymbols)
        //        {
        //            foreach (var requestMessageInterfaceSymbol in requestMessageSymbol.AllInterfaces)
        //            {
        //                if (requestMessageInterfaceSymbol.ContainingNamespace.Name != Constants.MediatorLib)
        //                    continue;

        //                if (_symbolComparer.Equals(baseRequestMessage, requestMessageInterfaceSymbol))
        //                    return baseRequestMessage.Construct()
        //            }
        //        }
        //    }

        //    static bool IsOpenGeneric(INamedTypeSymbol symbol) =>
        //        symbol.TypeArguments.Length > 0 && symbol.TypeArguments[0] is ITypeParameterSymbol;
        //}

        static bool IsOpenGeneric(INamedTypeSymbol symbol) =>
            symbol.TypeArguments.Length > 0 && symbol.TypeArguments[0] is ITypeParameterSymbol;

        private void TryParseOptions(CancellationToken cancellationToken)
        {
            var compilation = _compilation;

            var attrs = compilation.Assembly.GetAttributes();
            var optionsAttr = attrs.SingleOrDefault(a => a.AttributeClass?.Name == "MediatorOptions");
            if (optionsAttr is not null)
            {
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

        private delegate void ReportDiagnosticDelegate<T>(in GeneratorExecutionContext context, T state);
        private void ReportDiagnostic<T>(T state, ReportDiagnosticDelegate<T> del)
        {
            _hasErrors = true;
            del(in _context, state);
        }
    }
}
