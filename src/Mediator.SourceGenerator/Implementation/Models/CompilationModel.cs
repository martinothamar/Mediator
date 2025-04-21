namespace Mediator.SourceGenerator;

internal sealed record CompilationModel
{
    private const int ManyMessagesTreshold = 16;

    public CompilationModel(string mediatorNamespace, string generatorVersion)
    {
        MediatorNamespace = mediatorNamespace;
        GeneratorVersion = generatorVersion;
        HasErrors = true;
        IsTestRun = false;
        ConfiguredViaAttribute = false;
        ServiceLifetime = "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton";
        ServiceLifetimeShort = "Singleton";
        SingletonServiceLifetime = ServiceLifetime;
        ServiceLifetimeIsSingleton = true;
        ServiceLifetimeIsScoped = false;
        ServiceLifetimeIsTransient = false;
        ContainerMetadataField = "_containerMetadata.Value";
        InternalsNamespace = $"{MediatorNamespace}.Internals";
        TotalMessages = 0;
        NotificationPublisherType = new("global::Mediator.ForeachAwaitPublisher", "ForeachAwaitPublisher");

        RequestMessageHandlerWrappers = ImmutableEquatableArray<RequestMessageHandlerWrapperModel>.Empty;
        RequestMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        NotificationMessages = ImmutableEquatableArray<NotificationMessageModel>.Empty;
        NotificationMessageHandlers = ImmutableEquatableArray<NotificationMessageHandlerModel>.Empty;
        OpenGenericNotificationMessageHandlers = ImmutableEquatableArray<NotificationMessageHandlerModel>.Empty;

        IRequestMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        ICommandMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        IQueryMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        IStreamRequestMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        IStreamQueryMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        IStreamCommandMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
    }

    public CompilationModel(
        ImmutableEquatableArray<RequestMessageModel> requestMessages,
        ImmutableEquatableArray<NotificationMessageModel> notificationMessages,
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
        bool configuredViaAttribute
    )
    {
        MediatorNamespace = mediatorNamespace;
        GeneratorVersion = generatorVersion;
        HasErrors = hasErrors;
        IsTestRun = isTestRun;
        ConfiguredViaAttribute = configuredViaAttribute;
        ServiceLifetime = serviceLifetime;
        ServiceLifetimeShort = serviceLifetimeShort;
        SingletonServiceLifetime = singletonServiceLifetime;
        ServiceLifetimeIsSingleton = serviceLifetimeShort == "Singleton";
        ServiceLifetimeIsScoped = serviceLifetimeShort == "Scoped";
        ServiceLifetimeIsTransient = serviceLifetimeShort == "Transient";
        ContainerMetadataField = ServiceLifetimeIsSingleton ? "_containerMetadata.Value" : "_containerMetadata";
        InternalsNamespace = $"{MediatorNamespace}.Internals";
        TotalMessages = requestMessages.Count + notificationMessages.Count;
        NotificationPublisherType = notificationPublisherType;

        RequestMessageHandlerWrappers = requestMessageHandlerWrappers;
        RequestMessages = new(requestMessages.Where(r => r.Handler is not null));
        NotificationMessages = notificationMessages;
        NotificationMessageHandlers = new(notificationMessageHandlers.Where(h => !h.IsOpenGeneric));
        OpenGenericNotificationMessageHandlers = new(notificationMessageHandlers.Where(h => h.IsOpenGeneric));

        IRequestMessages = new(requestMessages.Where(r => r.Handler is not null && r.MessageType == "Request"));
        ICommandMessages = new(requestMessages.Where(r => r.Handler is not null && r.MessageType == "Command"));
        IQueryMessages = new(requestMessages.Where(r => r.Handler is not null && r.MessageType == "Query"));
        IStreamRequestMessages = new(
            requestMessages.Where(r => r.Handler is not null && r.MessageType == "StreamRequest")
        );
        IStreamQueryMessages = new(requestMessages.Where(r => r.Handler is not null && r.MessageType == "StreamQuery"));
        IStreamCommandMessages = new(
            requestMessages.Where(r => r.Handler is not null && r.MessageType == "StreamCommand")
        );

        HasRequests = requestMessages.Any(r => r.Handler is not null && r.MessageType == "Request");
        HasCommands = requestMessages.Any(r => r.Handler is not null && r.MessageType == "Command");
        HasQueries = requestMessages.Any(r => r.Handler is not null && r.MessageType == "Query");
        HasStreamRequests = requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamRequest");
        HasStreamQueries = requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamQuery");
        HasStreamCommands = requestMessages.Any(r => r.Handler is not null && r.MessageType == "StreamCommand");
        HasNotifications = notificationMessages.Any();

        var t = ManyMessagesTreshold;
        var m = requestMessages;
        HasManyRequests = m.Count(r => r.Handler is not null && r.MessageType == "Request") > t;
        HasManyCommands = m.Count(r => r.Handler is not null && r.MessageType == "Command") > t;
        HasManyQueries = m.Count(r => r.Handler is not null && r.MessageType == "Query") > t;
        HasManyStreamRequests = m.Count(r => r.Handler is not null && r.MessageType == "StreamRequest") > t;
        HasManyStreamQueries = m.Count(r => r.Handler is not null && r.MessageType == "StreamQuery") > t;
        HasManyStreamCommands = m.Count(r => r.Handler is not null && r.MessageType == "StreamCommand") > t;
        HasManyNotifications = notificationMessages.Count() > t;

        HasAnyRequest = HasRequests || HasCommands || HasQueries;
        HasAnyStreamRequest = HasStreamRequests || HasStreamQueries || HasStreamCommands;
        HasAnyValueTypeStreamResponse = requestMessages.Any(r =>
            r.MessageType.StartsWith("Stream") && r.ResponseIsValueType
        );
    }

    public string MediatorNamespace { get; }
    public string GeneratorVersion { get; }
    public bool HasErrors { get; }
    public bool IsTestRun { get; }
    public bool ConfiguredViaAttribute { get; }
    public string? ServiceLifetime { get; }
    public string? ServiceLifetimeShort { get; }
    public string? SingletonServiceLifetime { get; }
    public bool ServiceLifetimeIsSingleton { get; }
    public bool ServiceLifetimeIsScoped { get; }
    public bool ServiceLifetimeIsTransient { get; }
    public string ContainerMetadataField { get; }
    public string InternalsNamespace { get; }
    public int TotalMessages { get; }

    public NotificationPublisherTypeModel NotificationPublisherType { get; }

    public ImmutableEquatableArray<RequestMessageHandlerWrapperModel> RequestMessageHandlerWrappers { get; }

    public ImmutableEquatableArray<RequestMessageModel> RequestMessages { get; }

    public ImmutableEquatableArray<NotificationMessageModel> NotificationMessages { get; }

    public ImmutableEquatableArray<NotificationMessageHandlerModel> NotificationMessageHandlers { get; }

    public ImmutableEquatableArray<NotificationMessageHandlerModel> OpenGenericNotificationMessageHandlers { get; }

    public ImmutableEquatableArray<RequestMessageModel> IRequestMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> ICommandMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IQueryMessages { get; }

    public ImmutableEquatableArray<RequestMessageModel> IStreamRequestMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IStreamQueryMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IStreamCommandMessages { get; }

    public bool HasRequests { get; }
    public bool HasCommands { get; }
    public bool HasQueries { get; }
    public bool HasStreamRequests { get; }
    public bool HasStreamQueries { get; }
    public bool HasStreamCommands { get; }
    public bool HasNotifications { get; }

    public bool HasManyRequests { get; }
    public bool HasManyCommands { get; }
    public bool HasManyQueries { get; }
    public bool HasManyStreamRequests { get; }
    public bool HasManyStreamQueries { get; }
    public bool HasManyStreamCommands { get; }
    public bool HasManyNotifications { get; }

    public bool HasAnyRequest { get; }
    public bool HasAnyStreamRequest { get; }
    public bool HasAnyValueTypeStreamResponse { get; }
}
