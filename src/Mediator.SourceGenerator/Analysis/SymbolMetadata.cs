namespace Mediator.SourceGenerator;

internal abstract class SymbolMetadata<T> : IEquatable<T?> where T : SymbolMetadata<T>
{
    private static readonly SymbolEqualityComparer _comparer = SymbolEqualityComparer.Default;
    public readonly INamedTypeSymbol Symbol;
    protected readonly CompilationAnalyzer Analyzer;

    protected SymbolMetadata(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
    {
        Symbol = symbol;
        Analyzer = analyzer;
    }

    public override bool Equals(object? obj) => Equals(obj as T);

    public bool Equals(T? other) => other != null && _comparer.Equals(Symbol, other.Symbol);

    public override int GetHashCode() => _comparer.GetHashCode(Symbol);

    public override string ToString() => Symbol.Name;

    public bool IsStruct => Symbol.TypeKind == TypeKind.Struct;
    public bool IsClass => !IsStruct;
    public bool IsReadOnly => Symbol.IsReadOnly;
    public string ParameterModifier => string.Empty;
}
