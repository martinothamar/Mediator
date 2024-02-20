using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record RequestMessageModel : SymbolMetadataModel
{
    public RequestMessageModel(
        INamedTypeSymbol symbol,
        ITypeSymbol responseSymbol,
        string messageType,
        RequestMessageHandlerModel? handler,
        RequestMessageHandlerWrapperModel wrapperType
    ) : base(symbol)
    {
        ResponseIsValueType = responseSymbol.IsValueType;
        ResponseFullName = responseSymbol.GetTypeSymbolFullName();
        ResponseFullNameWithoutReferenceNullability = responseSymbol.GetTypeSymbolFullName(
            includeReferenceNullability: false
        );
        WrapperType = wrapperType;
        MessageType = messageType;
        Handler = handler;

        IdentifierFullName = symbol
            .GetTypeSymbolFullName(withGlobalPrefix: false, includeTypeParameters: false)
            .Replace("global::", "")
            .Replace('.', '_');

        HandlerWrapperTypeNameWithGenericTypeArguments = WrapperType.HandlerWrapperTypeNameWithGenericTypeArguments(
            symbol.TypeKind,
            RequestFullName,
            ResponseFullName
        );
    }

    public RequestMessageHandlerWrapperModel WrapperType { get; }

    public string MessageType { get; }

    public RequestMessageHandlerModel? Handler { get; }

    public string RequestFullName => FullName;
    public bool ResponseIsValueType { get; }
    public string ResponseFullName { get; }
    public string ResponseFullNameWithoutReferenceNullability { get; }

    public string HandlerWrapperTypeNameWithGenericTypeArguments { get; }

    public string IdentifierFullName { get; }

    public bool IsStreaming => MessageType.StartsWith("Stream");

    public string PipelineHandlerType =>
        IsStreaming
            ? $"global::Mediator.IStreamPipelineBehavior<{RequestFullName}, {ResponseFullName}>"
            : $"global::Mediator.IPipelineBehavior<{RequestFullName}, {ResponseFullName}>";

    public string HandlerWrapperPropertyName => $"Wrapper_For_{IdentifierFullName}";

    public string SyncMethodName => IsStreaming ? "CreateStream" : "Send";
    public string AsyncMethodName => IsStreaming ? "CreateStream" : "Send";

    public string SyncReturnType => ResponseFullName;
    public string AsyncReturnType =>
        IsStreaming
            ? $"global::System.Collections.Generic.IAsyncEnumerable<{ResponseFullName}>"
            : $"global::System.Threading.Tasks.ValueTask<{ResponseFullName}>";
}
