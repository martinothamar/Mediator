namespace Mediator.SourceGenerator;

internal sealed class NotificationMessageHandler : MessageHandler<NotificationMessageHandler>
{
    public NotificationMessageHandler(INamedTypeSymbol symbol, CompilationAnalyzer analyzer) : base(symbol, analyzer)
    { }

    public NotificationMessageHandlerModel ToModel()
    {
        return new NotificationMessageHandlerModel(Symbol, Analyzer);
    }
}
