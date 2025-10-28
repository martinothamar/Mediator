namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerWrapperModel
{
    public RequestMessageHandlerWrapperModel(string messageType, CompilationAnalyzer analyzer)
    {
        FullNamespace = $"global::{analyzer.MediatorNamespace}.Internals";
        MessageType = messageType;
        IsStreaming = messageType.StartsWith("Stream", StringComparison.Ordinal);
        TypeName = $"{messageType}HandlerWrapper";
        TypeNameWithGenericParameters = $"{TypeName}<TRequest, TResponse>";
        InterfaceTypeNameWithGenericParameter = $"I{messageType}HandlerBase<TResponse>";
    }

    public string FullNamespace { get; }

    public string MessageType { get; }

    public string TypeName { get; }

    public bool IsStreaming { get; }

    public string TypeNameWithGenericParameters { get; }

    public string InterfaceTypeNameWithGenericParameter { get; }

    public string MessageHandlerDelegateName =>
        IsStreaming
            ? $"global::Mediator.StreamHandlerDelegate<TRequest, TResponse>"
            : $"global::Mediator.MessageHandlerDelegate<TRequest, TResponse>";
    public string PipelineHandlerTypeName =>
        IsStreaming
            ? "global::Mediator.IStreamPipelineBehavior<TRequest, TResponse>"
            : "global::Mediator.IPipelineBehavior<TRequest, TResponse>";
    public string ReturnTypeName =>
        IsStreaming
            ? "global::System.Collections.Generic.IAsyncEnumerable<TResponse>"
            : "global::System.Threading.Tasks.ValueTask<TResponse>";
    public string ReturnTypeNameWhenObject =>
        IsStreaming
            ? "global::System.Collections.Generic.IAsyncEnumerable<object?>"
            : "global::System.Threading.Tasks.ValueTask<object?>";
    public string HandlerBase => IsStreaming ? "IStreamMessageHandlerBase" : "IMessageHandlerBase";
}
