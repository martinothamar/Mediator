using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal abstract record MessageHandlerModel : SymbolMetadataModel
{
    private readonly int _typeArgumentsLength;

    protected MessageHandlerModel(INamedTypeSymbol symbol, CompilationAnalyzer analyzer) : base(symbol)
    {
        ServiceLifetime = analyzer.ServiceLifetime;
        ServiceRegistrationBlock =
            $"services.TryAdd(new SD({TypeOfExpression(symbol)}, {TypeOfExpression(symbol)}, {ServiceLifetime}));";
        _typeArgumentsLength = symbol.TypeArguments.Length;
    }

    public bool IsOpenGeneric => _typeArgumentsLength > 0;

    public string ServiceRegistrationBlock { get; }

    public string? ServiceLifetime { get; }

    public static string TypeOfExpression(INamedTypeSymbol symbol, bool includeTypeParameters = true)
    {
        var typeName = symbol.GetTypeSymbolFullName(includeTypeParameters: includeTypeParameters);
        var genericArguments = string.Empty;
        if (symbol.TypeArguments.Length > 0 && !includeTypeParameters)
            genericArguments = $"<{new string(',', symbol.TypeArguments.Length - 1)}>";
        return $"typeof({typeName}{genericArguments})";
    }
}
