namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerModel : MessageHandlerModel
{
    public string MessageType { get; }
    public RequestMessageHandlerWrapperModel WrapperType { get; }

    public RequestMessageHandlerModel(
        INamedTypeSymbol symbol,
        string messageType,
        CompilationAnalyzer analyzer,
        RequestMessageHandlerWrapperModel wrapperType
    ) : base(symbol, analyzer)
    {
        MessageType = messageType;
        WrapperType = wrapperType;
    }
}
