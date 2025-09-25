namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerWrapperModel
{
    public RequestMessageHandlerWrapperModel(string messageType, CompilationAnalyzer analyzer)
    {
        FullNamespace = $"global::{analyzer.MediatorNamespace}.Internals";
        MessageType = messageType;
        VoidMessageType = messageType.Replace("Void", string.Empty);
        IsStreaming = messageType.StartsWith("Stream", StringComparison.Ordinal);
        IsVoid = messageType.StartsWith("Void", StringComparison.Ordinal);
        TypeName = $"{messageType}HandlerWrapper";
        TypeNameWithGenericParameters = IsVoid ? $"{TypeName}<TRequest>" : $"{TypeName}<TRequest, TResponse>";
        InterfaceTypeNameWithGenericParameter = IsVoid
            ? $"I{MessageType}HandlerBase"
            : $"I{messageType}HandlerBase<TResponse>";
    }

    public string FullNamespace { get; }
    public string MessageType { get; }
    public string VoidMessageType { get; }
    public string TypeName { get; }
    public bool IsStreaming { get; }

    public bool IsVoid { get; }

    public string TypeNameWithGenericParameters { get; }
    public string InterfaceTypeNameWithGenericParameter { get; }
    public string MessageHandlerDelegateName =>
        IsStreaming ? $"global::Mediator.StreamHandlerDelegate<TRequest, TResponse>"
        : IsVoid ? "global::Mediator.MessageHandlerDelegate<TRequest>"
        : $"global::Mediator.MessageHandlerDelegate<TRequest, TResponse>";
    public string PipelineHandlerTypeName =>
        IsStreaming ? "global::Mediator.IStreamPipelineBehavior<TRequest, TResponse>"
        : IsVoid ? "global::Mediator.IPipelineBehavior<TRequest>"
        : "global::Mediator.IPipelineBehavior<TRequest, TResponse>";
    public string ReturnTypeName =>
        IsStreaming ? "global::System.Collections.Generic.IAsyncEnumerable<TResponse>"
        : IsVoid ? "global::System.Threading.Tasks.ValueTask"
        : "global::System.Threading.Tasks.ValueTask<TResponse>";
    public string ReturnTypeNameWhenObject =>
        IsStreaming ? "global::System.Collections.Generic.IAsyncEnumerable<object?>"
        : IsVoid ? "global::System.Threading.Tasks.ValueTask"
        : "global::System.Threading.Tasks.ValueTask<object?>";
    public string HandlerBase =>
        IsStreaming ? "IStreamMessageHandlerBase"
        : IsVoid ? "IVoidMessageHandlerBase"
        : "IMessageHandlerBase";
}
