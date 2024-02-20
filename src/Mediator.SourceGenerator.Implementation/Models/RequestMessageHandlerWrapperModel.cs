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

    public string HandlerWrapperTypeName(TypeKind type) =>
        $"{MessageType}{(type == TypeKind.Struct ? "Struct" : "Class")}HandlerWrapper";

    private string HandlerWrapperTypeFullName(TypeKind type) => $"{FullNamespace}.{HandlerWrapperTypeName(type)}";

    private string HandlerWrapperTypeNameWithGenericTypeArguments(TypeKind type) =>
        $"{HandlerWrapperTypeName(type)}<TRequest, TResponse>";

    public string HandlerWrapperTypeNameWithGenericTypeArguments(
        TypeKind requestKind,
        string requestFullname,
        string responseFullname
    ) => $"{HandlerWrapperTypeFullName(requestKind)}<{requestFullname}, {responseFullname}>";

    public string HandlerWrapperTypeOfExpression(TypeKind type) => $"typeof({HandlerWrapperTypeFullName(type)}<,>)";

    public string ClassHandlerWrapperTypeNameWithGenericTypeArguments =>
        HandlerWrapperTypeNameWithGenericTypeArguments(TypeKind.Class);

    public string StructHandlerWrapperTypeNameWithGenericTypeArguments =>
        HandlerWrapperTypeNameWithGenericTypeArguments(TypeKind.Struct);

    public string ClassHandlerWrapperTypeName => HandlerWrapperTypeName(TypeKind.Class);

    public string StructHandlerWrapperTypeName => HandlerWrapperTypeName(TypeKind.Struct);

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
}
