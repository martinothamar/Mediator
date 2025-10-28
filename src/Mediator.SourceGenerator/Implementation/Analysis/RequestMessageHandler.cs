namespace Mediator.SourceGenerator;

internal sealed class RequestMessageHandler : MessageHandler<RequestMessageHandler>
{
    private readonly string _messageType;
    private readonly RequestMessageHandlerWrapperModel _wrapperType;

    public RequestMessageHandler(
        INamedTypeSymbol symbol,
        INamedTypeSymbol interfaceSymbol,
        string messageType,
        CompilationAnalyzer analyzer
    )
        : base(symbol, analyzer)
    {
        var hasResponse = interfaceSymbol.TypeArguments.Length == 2;
        _messageType = messageType;
        _wrapperType = analyzer.RequestMessageHandlerWrappers.Single(w =>
            w.MessageType == messageType && w.HasResponse == hasResponse
        );
    }

    public RequestMessageHandlerModel ToModel()
    {
        return new RequestMessageHandlerModel(Symbol, _messageType, Analyzer, _wrapperType);
    }
}
