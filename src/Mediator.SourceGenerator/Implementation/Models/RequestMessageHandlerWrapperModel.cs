namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerWrapperModel
{
    public RequestMessageHandlerWrapperModel(string messageType, CompilationAnalyzer analyzer, bool hasResponse = true)
    {
        HasResponse = hasResponse;
        FullNamespace = $"global::{analyzer.MediatorNamespace}.Internals";
        MessageType = messageType;
        IsStreaming = messageType.StartsWith("Stream", StringComparison.Ordinal);
        TypeName = $"{messageType}HandlerWrapper";
    }

    public string FullNamespace { get; }

    public string MessageType { get; }

    public string MessageTypeWithResponse => HasResponse ? $"{MessageType}<TResponse>" : MessageType;

    public string TypeName { get; }

    public bool IsStreaming { get; }

    public bool HasResponse { get; }

    public string TypeNameWithGenericParameters =>
        HasResponse ? $"{TypeName}<TRequest, TResponse>" : $"{TypeName}<TRequest>";

    public string UnitHandlerTypeName => $"Unit{TypeName}<TRequest>";

    public string InterfaceTypeNameWithGenericParameter =>
        HasResponse ? $"I{MessageType}HandlerBase<TResponse>" : $"I{MessageType}HandlerBase";

    public string MessageHandlerDelegateName =>
        IsStreaming ? $"global::Mediator.StreamHandlerDelegate<TRequest, TResponse>"
        : HasResponse ? "global::Mediator.MessageHandlerDelegate<TRequest, TResponse>"
        : "global::Mediator.MessageHandlerDelegate<TRequest, global::Mediator.Unit>";
    public string PipelineHandlerTypeName =>
        IsStreaming ? "global::Mediator.IStreamPipelineBehavior<TRequest, TResponse>"
        : HasResponse ? "global::Mediator.IPipelineBehavior<TRequest, TResponse>"
        : "global::Mediator.IPipelineBehavior<TRequest, global::Mediator.Unit>";
    public string InterfaceHandlerTypeName =>
        HasResponse
            ? $"global::Mediator.I{MessageType}Handler<TRequest, TResponse>"
            : $"{FullNamespace}.Unit{MessageType}HandlerWrapper<TRequest>";
    public string ReturnTypeName =>
        IsStreaming ? "global::System.Collections.Generic.IAsyncEnumerable<TResponse>"
        : HasResponse ? "global::System.Threading.Tasks.ValueTask<TResponse>"
        : "global::System.Threading.Tasks.ValueTask";
    public string ReturnTypeNameWhenObject =>
        IsStreaming ? "global::System.Collections.Generic.IAsyncEnumerable<object?>"
        : HasResponse ? "global::System.Threading.Tasks.ValueTask<object?>"
        : "global::System.Threading.Tasks.ValueTask";

    public string TaskReturnTypeNameWhenObject =>
        HasResponse
            ? "async global::System.Threading.Tasks.ValueTask<object?>"
            : "global::System.Threading.Tasks.ValueTask";

    public string HandlerBase =>
        IsStreaming ? "IStreamMessageHandlerBase"
        : HasResponse ? "IMessageHandlerBase"
        : "IUnitMessageHandlerBase";

    public string ReturnHandler =>
        HasResponse
            ? "handler(request, cancellationToken);"
            : "global::Mediator.ValueTaskTWrapper.AsValueTask<global::Mediator.Unit>(handler(request, cancellationToken));";
}
