using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal abstract record MessageHandlerModel : SymbolMetadataModel
{
    protected MessageHandlerModel(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
        : base(symbol)
    {
        var typeOfExpression = TypeOfExpression(symbol);
        ServiceRegistrationBlock =
            $"services.TryAdd(new SD({typeOfExpression}, {typeOfExpression}, {analyzer.ServiceLifetime}));";
        IsOpenGeneric = symbol.TypeArguments.Length > 0;
    }

    public bool IsOpenGeneric { get; }

    public string ServiceRegistrationBlock { get; }

    protected static string TypeOfExpression(INamedTypeSymbol symbol, bool includeTypeParameters = true)
    {
        var typeName = symbol.GetTypeSymbolFullName(includeTypeParameters: includeTypeParameters);
        var genericArguments = string.Empty;
        if (symbol.TypeArguments.Length > 0 && !includeTypeParameters)
            genericArguments = $"<{new string(',', symbol.TypeArguments.Length - 1)}>";
        return $"typeof({typeName}{genericArguments})";
    }
}
