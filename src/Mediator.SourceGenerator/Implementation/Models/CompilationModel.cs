namespace Mediator.SourceGenerator;

internal record CompilationModel
{
    private readonly ImmutableEquatableArray<RequestMessageModel> _requestMessages;
    private readonly ImmutableEquatableArray<NotificationMessageModel> _notificationMessages;
    private readonly ImmutableEquatableArray<RequestMessageHandlerModel> _requestMessageHandlers;
    private readonly ImmutableEquatableArray<NotificationMessageHandlerModel> _notificationMessageHandlers;

    public CompilationModel(string mediatorNamespace, string generatorVersion)
    {
        HasErrors = true;
        MediatorNamespace = mediatorNamespace;
        GeneratorVersion = generatorVersion;
        _requestMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        _notificationMessages = ImmutableEquatableArray<NotificationMessageModel>.Empty;
        _requestMessageHandlers = ImmutableEquatableArray<RequestMessageHandlerModel>.Empty;
        _notificationMessageHandlers = ImmutableEquatableArray<NotificationMessageHandlerModel>.Empty;
        RequestMessageHandlerWrappers = ImmutableEquatableArray<RequestMessageHandlerWrapperModel>.Empty;
    }

    public CompilationModel(
        ImmutableEquatableArray<RequestMessageModel> requestMessages,
        ImmutableEquatableArray<NotificationMessageModel> notificationMessages,
        ImmutableEquatableArray<RequestMessageHandlerModel> requestMessageHandlers,
        ImmutableEquatableArray<NotificationMessageHandlerModel> notificationMessageHandlers,
        ImmutableEquatableArray<RequestMessageHandlerWrapperModel> requestMessageHandlerWrappers,
        NotificationPublisherTypeModel notificationPublisherType,
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
        NotificationPublisherType = notificationPublisherType;
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

    public int TotalMessages => _requestMessages.Count + _notificationMessages.Count;

    public IEnumerable<RequestMessageHandlerModel> RequestMessageHandlers => _requestMessageHandlers;

    public NotificationPublisherTypeModel NotificationPublisherType { get; }

    public bool HasErrors { get; }

    public IEnumerable<NotificationMessageHandlerModel> NotificationMessageHandlers =>
        _notificationMessageHandlers.Where(h => !h.IsOpenGeneric);

    public IEnumerable<NotificationMessageHandlerModel> OpenGenericNotificationMessageHandlers =>
        _notificationMessageHandlers.Where(h => h.IsOpenGeneric);

    public bool HasRequests => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "Request");
    public bool HasCommands => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "Command");
    public bool HasQueries => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "Query");

    private const int ManyMessagesTreshold = 16;

    public bool HasManyRequests =>
        _requestMessages.Count(r => r.Handler is not null && r.MessageType == "Request") > ManyMessagesTreshold;
    public bool HasManyCommands =>
        _requestMessages.Count(r => r.Handler is not null && r.MessageType == "Command") > ManyMessagesTreshold;
    public bool HasManyQueries =>
        _requestMessages.Count(r => r.Handler is not null && r.MessageType == "Query") > ManyMessagesTreshold;

    public bool HasStreamRequests =>
        _requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamRequest");
    public bool HasStreamQueries => _requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamQuery");
    public bool HasStreamCommands =>
        _requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamCommand");

    public bool HasManyStreamRequests =>
        _requestMessages.Count(r => r.Handler is not null && r.MessageType == "StreamRequest") > ManyMessagesTreshold;
    public bool HasManyStreamQueries =>
        _requestMessages.Count(r => r.Handler is not null && r.MessageType == "StreamQuery") > ManyMessagesTreshold;
    public bool HasManyStreamCommands =>
        _requestMessages.Count(r => r.Handler is not null && r.MessageType == "StreamCommand") > ManyMessagesTreshold;

    public bool HasAnyRequest => HasRequests || HasCommands || HasQueries;

    public bool HasAnyStreamRequest => HasStreamRequests || HasStreamQueries || HasStreamCommands;

    public bool HasAnyValueTypeStreamResponse =>
        _requestMessages.Any(r => r.MessageType.StartsWith("Stream") && r.ResponseIsValueType);

    public bool HasNotifications => _notificationMessages.Any();
    public bool HasManyNotifications => _notificationMessages.Count() > ManyMessagesTreshold;

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

    public string ContainerMetadataField =>
        ServiceLifetimeIsSingleton ? "_containerMetadata.Value" : "_containerMetadata";

    public bool ServiceLifetimeIsScoped => ServiceLifetimeShort == "Scoped";

    public bool ServiceLifetimeIsTransient => ServiceLifetimeShort == "Transient";

    public bool IsTestRun { get; }

    public bool ConfiguredViaAttribute { get; }

    public bool ConfiguredViaConfiguration { get; }

    public string InternalsNamespace => $"{MediatorNamespace}.Internals";
}
