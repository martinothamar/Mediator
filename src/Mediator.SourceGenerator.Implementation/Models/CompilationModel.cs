namespace Mediator.SourceGenerator;

internal record CompilationModel
{
    private readonly ImmutableEquatableArray<RequestMessageModel> _requestMessages;
    private readonly ImmutableEquatableArray<NotificationMessageModel> _notificationMessages;
    private readonly ImmutableEquatableArray<RequestMessageHandlerModel> _requestMessageHandlers;
    private readonly ImmutableEquatableArray<NotificationMessageHandlerModel> _notificationMessageHandlers;

    public CompilationModel(
        ImmutableEquatableArray<RequestMessageModel> requestMessages,
        ImmutableEquatableArray<NotificationMessageModel> notificationMessages,
        ImmutableEquatableArray<RequestMessageHandlerModel> requestMessageHandlers,
        ImmutableEquatableArray<NotificationMessageHandlerModel> notificationMessageHandlers,
        ImmutableEquatableArray<RequestMessageHandlerWrapperModel> requestMessageHandlerWrappers,
        bool hasErrors,
        string mediatorNamespace,
        string generatorVersion,
        string? serviceLifetime,
        string? serviceLifetimeShort,
        string? singletonServiceLifetime,
        bool isTestRun,
        bool configuredViaAttribute,
        bool configuredViaConfiguration
    )
    {
        _requestMessages = requestMessages;
        _notificationMessages = notificationMessages;
        _requestMessageHandlers = requestMessageHandlers;
        _notificationMessageHandlers = notificationMessageHandlers;
        RequestMessageHandlerWrappers = requestMessageHandlerWrappers;
        HasErrors = hasErrors;
        MediatorNamespace = mediatorNamespace;
        GeneratorVersion = generatorVersion;
        ServiceLifetime = serviceLifetime;
        ServiceLifetimeShort = serviceLifetimeShort;
        SingletonServiceLifetime = singletonServiceLifetime;
        IsTestRun = isTestRun;
        ConfiguredViaAttribute = configuredViaAttribute;
        ConfiguredViaConfiguration = configuredViaConfiguration;
    }

    public ImmutableEquatableArray<RequestMessageHandlerWrapperModel> RequestMessageHandlerWrappers { get; }

    public IEnumerable<RequestMessageModel> RequestMessages => _requestMessages.Where(r => r.Handler is not null);

    public IEnumerable<NotificationMessageModel> NotificationMessages => _notificationMessages;

    public IEnumerable<RequestMessageHandlerModel> RequestMessageHandlers => _requestMessageHandlers;

    public bool HasErrors { get; }

    public IEnumerable<NotificationMessageHandlerModel> NotificationMessageHandlers =>
        _notificationMessageHandlers.Where(h => !h.IsOpenGeneric);

    public IEnumerable<NotificationMessageHandlerModel> OpenGenericNotificationMessageHandlers =>
        _notificationMessageHandlers.Where(h => h.IsOpenGeneric);

    public bool HasRequests => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "Request");
    public bool HasCommands => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "Command");
    public bool HasQueries => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "Query");

    public bool HasStreamRequests =>
        _requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamRequest");
    public bool HasStreamQueries => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamQuery");
    public bool HasStreamCommands =>
        _requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamCommand");

    public bool HasAnyRequest => HasRequests || HasCommands || HasQueries;

    public bool HasAnyStreamRequest => HasStreamRequests || HasStreamQueries || HasStreamCommands;

    public bool HasAnyValueTypeStreamResponse =>
        _requestMessages.Any(r => r.MessageType.StartsWith("Stream") && r.ResponseIsValueType);

    public bool HasNotifications => _notificationMessages.Any();

    public IEnumerable<RequestMessageModel> IRequestMessages =>
        _requestMessages.Where(r => r.Handler is not null && r.MessageType == "Request");
    public IEnumerable<RequestMessageModel> ICommandMessages =>
        _requestMessages.Where(r => r.Handler is not null && r.MessageType == "Command");
    public IEnumerable<RequestMessageModel> IQueryMessages =>
        _requestMessages.Where(r => r.Handler is not null && r.MessageType == "Query");

    public IEnumerable<RequestMessageModel> IStreamRequestMessages =>
        _requestMessages.Where(r => r.Handler is not null && r.MessageType == "StreamRequest");
    public IEnumerable<RequestMessageModel> IStreamQueryMessages =>
        _requestMessages.Where(r => r.Handler is not null && r.MessageType == "StreamQuery");
    public IEnumerable<RequestMessageModel> IStreamCommandMessages =>
        _requestMessages.Where(r => r.Handler is not null && r.MessageType == "StreamCommand");

    public IEnumerable<RequestMessageModel> IMessages =>
        _requestMessages.Where(r => r.Handler is not null && !r.IsStreaming);
    public IEnumerable<RequestMessageModel> IStreamMessages =>
        _requestMessages.Where(r => r.Handler is not null && r.IsStreaming);

    public string MediatorNamespace { get; }
    public string GeneratorVersion { get; }

    public string? ServiceLifetime { get; }

    public string? ServiceLifetimeShort { get; }

    public string? SingletonServiceLifetime { get; }

    public bool ServiceLifetimeIsSingleton => ServiceLifetimeShort == "Singleton";

    public bool ServiceLifetimeIsScoped => ServiceLifetimeShort == "Scoped";

    public bool ServiceLifetimeIsTransient => ServiceLifetimeShort == "Transient";

    public bool IsTestRun { get; }

    public bool ConfiguredViaAttribute { get; }

    public bool ConfiguredViaConfiguration { get; }
}
