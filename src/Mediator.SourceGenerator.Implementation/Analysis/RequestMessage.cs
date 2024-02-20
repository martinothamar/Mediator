namespace Mediator.SourceGenerator;

internal sealed class RequestMessage : SymbolMetadata<RequestMessage>
{
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

    public RequestMessageHandler? Handler { get; private set; }

    public ITypeSymbol ResponseSymbol { get; }

    public RequestMessageHandlerWrapperModel WrapperType { get; }

    public string MessageType { get; }

    public void SetHandler(RequestMessageHandler handler) => Handler = handler;
}
