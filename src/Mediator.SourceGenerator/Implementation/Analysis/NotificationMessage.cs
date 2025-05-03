namespace Mediator.SourceGenerator;

internal sealed class NotificationMessage : SymbolMetadata<NotificationMessage>
{
    public NotificationMessage(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
        : base(symbol, analyzer) { }

    public NotificationMessageModel ToModel()
    {
        return new NotificationMessageModel(Symbol, Analyzer);
    }
}
