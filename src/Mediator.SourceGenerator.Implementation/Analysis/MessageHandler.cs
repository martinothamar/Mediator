using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal abstract class MessageHandler<T> : SymbolMetadata<MessageHandler<T>>
{
    protected MessageHandler(INamedTypeSymbol symbol, CompilationAnalyzer analyzer) : base(symbol, analyzer) { }

    public bool IsOpenGeneric => Symbol.TypeArguments.Length > 0;

    public string FullName => Symbol.GetTypeSymbolFullName();

    public string TypeOfExpression(bool includeTypeParameters = true)
    {
        var typeName = Symbol.GetTypeSymbolFullName(includeTypeParameters: includeTypeParameters);
        var genericArguments = string.Empty;
        if (IsOpenGeneric && !includeTypeParameters)
            genericArguments = $"<{new string(',', Symbol.TypeArguments.Length - 1)}>";
        return $"typeof({typeName}{genericArguments})";
    }

    public string ServiceRegistrationBlock =>
        $"services.TryAdd(new SD({TypeOfExpression()}, {TypeOfExpression()}, {ServiceLifetime}));";

    public string? ServiceLifetime => Analyzer.ServiceLifetime;
}
