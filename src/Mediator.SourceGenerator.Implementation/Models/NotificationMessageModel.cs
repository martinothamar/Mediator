using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record NotificationMessageModel : SymbolMetadataModel
{
    private readonly ImmutableEquatableArray<string> _handlers;

    public NotificationMessageModel(
        INamedTypeSymbol symbol,
        CompilationAnalyzer analyzer,
        HashSet<NotificationMessageHandler> handlers
    ) : base(symbol)
    {
        ServiceLifetime = analyzer.ServiceLifetime;
        _handlers = handlers.Select(x => x.FullName).ToImmutableEquatableArray();
        IdentifierFullName = symbol
            .GetTypeSymbolFullName(withGlobalPrefix: false, includeTypeParameters: false)
            .Replace("global::", "")
            .Replace('.', '_');
    }

    public string IdentifierFullName { get; }

    public int HandlerCount => _handlers.Count;

    public string? ServiceLifetime { get; }

    public string HandlerTypeOfExpression => $"typeof(global::Mediator.INotificationHandler<{FullName}>)";

    public IEnumerable<string> HandlerServicesRegistrationBlock
    {
        get
        {
            var handlerTypeOfExpression = HandlerTypeOfExpression;
            foreach (var handler in _handlers)
            {
                var getExpression = $"GetRequiredService<{handler}>()";
                yield return $"services.Add(new SD({handlerTypeOfExpression}, {getExpression}, {ServiceLifetime}));";
            }
        }
    }
}
