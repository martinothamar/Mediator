using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed class RequestMessageHandlerWrapper
{
    public readonly string MessageType;
    private readonly CompilationAnalyzer _analyzer;

    public RequestMessageHandlerWrapper(string messageType, CompilationAnalyzer analyzer)
    {
        MessageType = messageType;
        _analyzer = analyzer;
    }

    public string FullNamespace => $"global::{_analyzer.MediatorNamespace}";

    public string HandlerWrapperTypeName(TypeKind type) =>
        $"{MessageType}{(type == TypeKind.Struct ? "Struct" : "Class")}HandlerWrapper";

    public string HandlerWrapperTypeFullName(TypeKind type) => $"{FullNamespace}.{HandlerWrapperTypeName(type)}";

    public string HandlerWrapperTypeNameWithGenericTypeArguments(TypeKind type) =>
        $"{HandlerWrapperTypeName(type)}<TRequest, TResponse>";

    public string HandlerWrapperTypeNameWithGenericTypeArguments(
        INamedTypeSymbol requestSymbol,
        ITypeSymbol responseSymbol
    ) =>
        $"{HandlerWrapperTypeFullName(requestSymbol.TypeKind)}<{requestSymbol.GetTypeSymbolFullName()}, {responseSymbol.GetTypeSymbolFullName()}>";

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
