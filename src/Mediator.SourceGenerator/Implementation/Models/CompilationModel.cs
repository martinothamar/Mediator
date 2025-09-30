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
        TypeAccessibility = "public";
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
        PipelineBehaviors = ImmutableEquatableArray<PipelineBehaviorModel>.Empty;

        IRequestMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        ICommandMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        IVoidRequestMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
        IVoidCommandMessages = ImmutableEquatableArray<RequestMessageModel>.Empty;
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
        ImmutableEquatableArray<PipelineBehaviorModel> pipelineBehaviors,
        bool hasErrors,
        string mediatorNamespace,
        string generatorVersion,
        string? serviceLifetime,
        string? serviceLifetimeShort,
        string? singletonServiceLifetime,
        bool isTestRun,
        bool configuredViaAttribute,
        bool generateTypesAsInternal
    )
    {
        MediatorNamespace = mediatorNamespace;
        GeneratorVersion = generatorVersion;
        HasErrors = hasErrors;
        IsTestRun = isTestRun;
        ConfiguredViaAttribute = configuredViaAttribute;
        TypeAccessibility = generateTypesAsInternal ? "internal" : "public";
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
        PipelineBehaviors = pipelineBehaviors;

        RequestMessageHandlerWrappers = requestMessageHandlerWrappers;
        NotificationMessages = notificationMessages;
        NotificationMessageHandlers = new(notificationMessageHandlers);

        var reqMessages = new List<RequestMessageModel>();

        var iRequestMessages = new List<RequestMessageModel>();
        var iCommandMessages = new List<RequestMessageModel>();
        var iVoidRequestMessages = new List<RequestMessageModel>();
        var iVoidCommandMessages = new List<RequestMessageModel>();
        var iQueryMessages = new List<RequestMessageModel>();
        var iStreamRequestMessages = new List<RequestMessageModel>();
        var iStreamQueryMessages = new List<RequestMessageModel>();
        var iStreamCommandMessages = new List<RequestMessageModel>();

        for (int i = 0; i < requestMessages.Count; i++)
        {
            var r = requestMessages[i];
            if (r.Handler is not null)
            {
                reqMessages.Add(r);
                var list = r.MessageKind switch
                {
                    RequestMessageKind.Request => iRequestMessages,
                    RequestMessageKind.Command => iCommandMessages,
                    RequestMessageKind.VoidRequest => iVoidRequestMessages,
                    RequestMessageKind.VoidCommand => iVoidCommandMessages,
                    RequestMessageKind.Query => iQueryMessages,
                    RequestMessageKind.StreamRequest => iStreamRequestMessages,
                    RequestMessageKind.StreamQuery => iStreamQueryMessages,
                    RequestMessageKind.StreamCommand => iStreamCommandMessages,
                    _ => throw new ArgumentOutOfRangeException(nameof(r.MessageKind), r.MessageKind, null),
                };
                list.Add(r);
            }

            var isStreaming =
                r.MessageKind
                    is RequestMessageKind.StreamRequest
                        or RequestMessageKind.StreamQuery
                        or RequestMessageKind.StreamCommand;
            if (isStreaming && r.ResponseIsValueType)
                HasAnyValueTypeStreamResponse = true;
        }

        RequestMessages = new(reqMessages);

        IRequestMessages = new(iRequestMessages);
        ICommandMessages = new(iCommandMessages);
        IVoidCommandMessages = new(iVoidCommandMessages);
        IVoidRequestMessages = new(iVoidRequestMessages);
        IQueryMessages = new(iQueryMessages);
        IStreamRequestMessages = new(iStreamRequestMessages);
        IStreamQueryMessages = new(iStreamQueryMessages);
        IStreamCommandMessages = new(iStreamCommandMessages);

        HasRequests = iRequestMessages.Count > 0;
        HasCommands = iCommandMessages.Count > 0;
        HasQueries = iQueryMessages.Count > 0;
        HasVoidRequests = iVoidRequestMessages.Count > 0;
        HasVoidCommands = iVoidCommandMessages.Count > 0;
        HasStreamRequests = iStreamRequestMessages.Count > 0;
        HasStreamQueries = iStreamQueryMessages.Count > 0;
        HasStreamCommands = iStreamCommandMessages.Count > 0;
        HasNotifications = notificationMessages.Count > 0;

        HasManyRequests = iRequestMessages.Count > ManyMessagesTreshold;
        HasManyVoidRequests = iVoidRequestMessages.Count > ManyMessagesTreshold;
        HasManyCommands = iCommandMessages.Count > ManyMessagesTreshold;
        HasManyVoidCommands = iVoidCommandMessages.Count > ManyMessagesTreshold;
        HasManyQueries = iQueryMessages.Count > ManyMessagesTreshold;
        HasManyStreamRequests = iStreamRequestMessages.Count > ManyMessagesTreshold;
        HasManyStreamQueries = iStreamQueryMessages.Count > ManyMessagesTreshold;
        HasManyStreamCommands = iStreamCommandMessages.Count > ManyMessagesTreshold;
        HasManyNotifications = notificationMessages.Count > ManyMessagesTreshold;

        HasAnyRequest = HasRequests || HasCommands || HasQueries || HasVoidCommands || HasVoidRequests;
        HasAnyStreamRequest = HasStreamRequests || HasStreamQueries || HasStreamCommands;
    }

    public string MediatorNamespace { get; }
    public string GeneratorVersion { get; }
    public bool HasErrors { get; }
    public bool IsTestRun { get; }
    public bool ConfiguredViaAttribute { get; }
    public string TypeAccessibility { get; }
    public string? ServiceLifetime { get; }
    public string? ServiceLifetimeShort { get; }
    public string? SingletonServiceLifetime { get; }
    public string SD => "global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor";
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

    public ImmutableEquatableArray<PipelineBehaviorModel> PipelineBehaviors { get; }

    public ImmutableEquatableArray<RequestMessageModel> IRequestMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> ICommandMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IVoidRequestMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IVoidCommandMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IQueryMessages { get; }

    public ImmutableEquatableArray<RequestMessageModel> IStreamRequestMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IStreamQueryMessages { get; }
    public ImmutableEquatableArray<RequestMessageModel> IStreamCommandMessages { get; }

    public bool HasRequests { get; }
    public bool HasVoidRequests { get; }
    public bool HasCommands { get; }
    public bool HasVoidCommands { get; }
    public bool HasQueries { get; }
    public bool HasStreamRequests { get; }
    public bool HasStreamQueries { get; }
    public bool HasStreamCommands { get; }
    public bool HasNotifications { get; }

    public bool HasManyRequests { get; }
    public bool HasManyVoidRequests { get; }
    public bool HasManyCommands { get; }
    public bool HasManyVoidCommands { get; }
    public bool HasManyQueries { get; }
    public bool HasManyStreamRequests { get; }
    public bool HasManyStreamQueries { get; }
    public bool HasManyStreamCommands { get; }
    public bool HasManyNotifications { get; }

    public bool HasAnyRequest { get; }
    public bool HasAnyStreamRequest { get; }
    public bool HasAnyValueTypeStreamResponse { get; }
}
