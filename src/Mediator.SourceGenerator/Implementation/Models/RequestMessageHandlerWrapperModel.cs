namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerWrapperModel
{
    public RequestMessageHandlerWrapperModel(string messageType, CompilationAnalyzer analyzer)
    {
        MessageType = messageType;
        FullNamespace = $"global::{analyzer.MediatorNamespace}";
    }

    public string MessageType { get; }

    public string FullNamespace { get; }

    public string HandlerWrapperTypeNameImpl() => $"{MessageType}HandlerWrapper";

    private string HandlerWrapperTypeFullName() => $"{FullNamespace}.{HandlerWrapperTypeNameImpl()}";

    public string HandlerWrapperTypeNameWithGenericTypeArguments(string requestFullname, string responseFullname) =>
        $"{HandlerWrapperTypeFullName()}<{requestFullname}, {responseFullname}>";

    public string HandlerWrapperTypeOfExpression() => $"typeof({HandlerWrapperTypeFullName()}<,>)";

    public string HandlerWrapperTypeNameWithGenericTypeParameters =>
        $"{HandlerWrapperTypeNameImpl()}<TRequest, TResponse>";

    public string HandlerWrapperTypeName => HandlerWrapperTypeNameImpl();

    public bool IsStreaming => MessageType.StartsWith("Stream");

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
