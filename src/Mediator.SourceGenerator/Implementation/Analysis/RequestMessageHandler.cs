namespace Mediator.SourceGenerator;

internal sealed class RequestMessageHandler : MessageHandler<RequestMessageHandler>
{
    private readonly string _messageType;
    private readonly RequestMessageHandlerWrapperModel _wrapperType;

    public RequestMessageHandler(INamedTypeSymbol symbol, string messageType, CompilationAnalyzer analyzer)
        : base(symbol, analyzer)
    {
        _messageType = messageType;
        _wrapperType = analyzer.RequestMessageHandlerWrappers.Single(w => w.MessageType == messageType);
    }

    public RequestMessageHandlerModel ToModel()
    {
        return new RequestMessageHandlerModel(Symbol, _messageType, Analyzer, _wrapperType);
    }
}
