using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed class RequestMessage : SymbolMetadata<RequestMessage>
{
    public RequestMessageHandler? Handler { get; private set; }

    public readonly ITypeSymbol ResponseSymbol;

    public readonly RequestMessageHandlerWrapper WrapperType;

    public readonly string MessageType;

    public bool IsStreaming => MessageType.StartsWith("Stream");

    public RequestMessage(
        INamedTypeSymbol symbol,
        ITypeSymbol responseSymbol,
        string messageType,
        CompilationAnalyzer analyzer
    ) : base(symbol, analyzer)
    {
        ResponseSymbol = responseSymbol;
        WrapperType = analyzer.RequestMessageHandlerWrappers.Single(w => w.MessageType == messageType);
        MessageType = messageType;
    }

    public string RequestFullName => Symbol.GetTypeSymbolFullName();
    public bool ResponseIsValueType => ResponseSymbol!.IsValueType;
    public string ResponseFullName => ResponseSymbol!.GetTypeSymbolFullName();

    public void SetHandler(RequestMessageHandler handler) => Handler = handler;

    public string HandlerWrapperTypeNameWithGenericTypeArguments =>
        WrapperType.HandlerWrapperTypeNameWithGenericTypeArguments(Symbol, ResponseSymbol);

    public string PipelineHandlerType =>
        IsStreaming
            ? $"global::Mediator.IStreamPipelineBehavior<{RequestFullName}, {ResponseFullName}>"
            : $"global::Mediator.IPipelineBehavior<{RequestFullName}, {ResponseFullName}>";

    public string IdentifierFullName =>
        Symbol
            .GetTypeSymbolFullName(withGlobalPrefix: false, includeTypeParameters: false)
            .Replace("global::", "")
            .Replace('.', '_');

    public string HandlerWrapperPropertyName => $"Wrapper_For_{IdentifierFullName}";

    public string SyncMethodName => IsStreaming ? "CreateStream" : "Send";
    public string AsyncMethodName => IsStreaming ? "CreateStream" : "Send";

    public string SyncReturnType => ResponseSymbol.GetTypeSymbolFullName();
    public string AsyncReturnType =>
        IsStreaming
            ? $"global::System.Collections.Generic.IAsyncEnumerable<{ResponseFullName}>"
            : $"global::System.Threading.Tasks.ValueTask<{ResponseFullName}>";
}
