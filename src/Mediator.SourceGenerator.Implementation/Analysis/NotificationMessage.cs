namespace Mediator.SourceGenerator;

internal sealed class NotificationMessage : SymbolMetadata<NotificationMessage>
{
    private readonly HashSet<NotificationMessageHandler> _handlers;

    public NotificationMessage(INamedTypeSymbol symbol, CompilationAnalyzer analyzer) : base(symbol, analyzer)
    {
        _handlers = new();
    }

    internal void AddHandlers(NotificationMessageHandler handler) => _handlers.Add(handler);

    public NotificationMessageModel ToModel()
    {
        return new NotificationMessageModel(Symbol, Analyzer, _handlers);
    }
}
