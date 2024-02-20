using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal abstract record SymbolMetadataModel
{
    protected SymbolMetadataModel(INamedTypeSymbol symbol)
    {
        Name = symbol.Name;
        FullName = symbol.GetTypeSymbolFullName();
        Kind = symbol.TypeKind;
        IsReadOnly = symbol.IsReadOnly;
        AccessibilityModifier = symbol.DeclaredAccessibility is Accessibility.Internal ? "internal" : "public";
    }

    public string Name { get; }

    public string FullName { get; }

    public TypeKind Kind { get; }

    public bool IsStruct => Kind == TypeKind.Struct;

    public bool IsClass => !IsStruct;

    public bool IsReadOnly { get; }

    public string AccessibilityModifier { get; }

    public string ParameterModifier => string.Empty;

    public override string ToString() => Name;
}
