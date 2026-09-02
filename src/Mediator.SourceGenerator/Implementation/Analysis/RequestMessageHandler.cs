namespace Mediator.SourceGenerator;

internal sealed class RequestMessageHandler : MessageHandler<RequestMessageHandler>
{
    private readonly string _messageType;

    public RequestMessageHandler(INamedTypeSymbol symbol, string messageType, CompilationAnalyzer analyzer)
        : base(symbol, analyzer)
    {
        _messageType = messageType;
    }

    public RequestMessageHandlerModel ToModel(INamedTypeSymbol messageSymbol, ITypeSymbol responseSymbol)
    {
        return new RequestMessageHandlerModel(Symbol, messageSymbol, responseSymbol, _messageType, Analyzer);
    }
}
