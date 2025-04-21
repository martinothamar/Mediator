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
    )
        : base(symbol)
    {
        ResponseIsValueType = responseSymbol.IsValueType;
        ResponseFullName = responseSymbol.GetTypeSymbolFullName();
        ResponseFullNameWithoutReferenceNullability = responseSymbol.GetTypeSymbolFullName(
            includeReferenceNullability: false
        );
        MessageType = messageType;
        Handler = handler;

        var fullHandlerWrapperTypeName = $"{wrapperType.FullNamespace}.{wrapperType.TypeName}";
        HandlerWrapperTypeNameWithGenericTypeArguments =
            $"{fullHandlerWrapperTypeName}<{FullName}, {ResponseFullName}>";

        var identifierFullName = symbol
            .GetTypeSymbolFullName(withGlobalPrefix: false, includeTypeParameters: false)
            .Replace("global::", "")
            .Replace('.', '_');
        var isStreaming = MessageType.StartsWith("Stream", StringComparison.Ordinal);

        HandlerWrapperPropertyName = $"Wrapper_For_{identifierFullName}";
        MethodName = isStreaming ? "CreateStream" : "Send";
        ReturnType = isStreaming
            ? $"global::System.Collections.Generic.IAsyncEnumerable<{ResponseFullName}>"
            : $"global::System.Threading.Tasks.ValueTask<{ResponseFullName}>";
    }

    public string MessageType { get; }
    public RequestMessageHandlerModel? Handler { get; }
    public bool ResponseIsValueType { get; }
    public string ResponseFullName { get; }
    public string ResponseFullNameWithoutReferenceNullability { get; }
    public string HandlerWrapperTypeNameWithGenericTypeArguments { get; }
    public string HandlerWrapperPropertyName { get; }
    public string MethodName { get; }
    public string ReturnType { get; }
}
