namespace Mediator.SourceGenerator;

internal readonly struct MediatorGenerationContext
{
    public readonly IReadOnlyDictionary<INamedTypeSymbol, List<INamedTypeSymbol>> HandlerMap;
    public readonly IEnumerable<INamedTypeSymbol> HandlerTypes;
    public readonly string MediatorNamespace;

    public MediatorGenerationContext(
        IReadOnlyDictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlerMap,
        IEnumerable<INamedTypeSymbol> handlerTypes,
        string mediatorNamespace
    )
    {
        HandlerMap = handlerMap;
        HandlerTypes = handlerTypes;
        MediatorNamespace = mediatorNamespace;
    }
}
