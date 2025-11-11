using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal enum RequestMessageKind
{
    Request,
    Query,
    Command,
    StreamRequest,
    StreamQuery,
    StreamCommand,
}

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
        MessageType = messageType;
        MessageKind = messageType switch
        {
            "Request" => RequestMessageKind.Request,
            "Query" => RequestMessageKind.Query,
            "Command" => RequestMessageKind.Command,
            "StreamRequest" => RequestMessageKind.StreamRequest,
            "StreamQuery" => RequestMessageKind.StreamQuery,
            "StreamCommand" => RequestMessageKind.StreamCommand,
            _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null),
        };
        var isStreaming =
            MessageKind
            is RequestMessageKind.StreamRequest
                or RequestMessageKind.StreamQuery
                or RequestMessageKind.StreamCommand;

        ResponseIsValueType = responseSymbol.IsValueType;
        ResponseFullName = responseSymbol.GetTypeSymbolFullName();
        ResponseFullNameWithoutReferenceNullability = responseSymbol.GetTypeSymbolFullName(
            includeReferenceNullability: false
        );
        Handler = handler;

        var fullHandlerWrapperTypeName = $"{wrapperType.FullNamespace}.{wrapperType.TypeName}";
        HandlerWrapperTypeNameWithGenericTypeArguments =
            $"{fullHandlerWrapperTypeName}<{FullName}, {ResponseFullName}>";

        var identifierFullName = symbol
            .GetTypeSymbolFullName(withGlobalPrefix: false, includeTypeParameters: false)
            .Replace("global::", "")
            .Replace('.', '_');

        HandlerWrapperPropertyName = $"Wrapper_For_{identifierFullName}";
        MethodName = isStreaming ? "CreateStream" : "Send";
        ReturnType = isStreaming
            ? $"global::System.Collections.Generic.IAsyncEnumerable<{ResponseFullName}>"
            : $"global::System.Threading.Tasks.ValueTask<{ResponseFullName}>";
    }

    public string MessageType { get; }
    public RequestMessageKind MessageKind { get; }
    public RequestMessageHandlerModel? Handler { get; }
    public bool ResponseIsValueType { get; }
    public string ResponseFullName { get; }
    public string ResponseFullNameWithoutReferenceNullability { get; }
    public string HandlerWrapperTypeNameWithGenericTypeArguments { get; }
    public string HandlerWrapperPropertyName { get; }
    public string MethodName { get; }
    public string ReturnType { get; }
}
