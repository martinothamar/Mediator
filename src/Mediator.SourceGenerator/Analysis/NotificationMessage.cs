using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed class NotificationMessage : SymbolMetadata<NotificationMessage>
{
    private readonly HashSet<NotificationMessageHandler> _handlers;

    public NotificationMessage(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
        : base(symbol, analyzer)
    {
        _handlers = new();
    }

    internal void AddHandlers(NotificationMessageHandler handler) => _handlers.Add(handler);

    public string FullName => Symbol.GetTypeSymbolFullName();

    public int HandlerCount => _handlers.Count;

    public string ServiceLifetime => Analyzer.ServiceLifetime;

    public string HandlerTypeOfExpression => $"typeof(global::Mediator.INotificationHandler<{Symbol.GetTypeSymbolFullName()}>)";

    public IEnumerable<string> HandlerServicesRegistrationBlock
    {
        get
        {
            foreach (var handler in _handlers)
            {
                var getExpression = $"sp => sp.GetRequiredService<{handler.FullName}>()";
                yield return $"services.Add(new SD({HandlerTypeOfExpression}, {getExpression}, {ServiceLifetime}));";
            }
        }
    }
}
