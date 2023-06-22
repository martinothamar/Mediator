namespace Mediator.SourceGenerator.Extensions;

public static class RoslynExtensions
{
    public static string GetTypeSymbolFullName(
        this ITypeSymbol symbol,
        bool withGlobalPrefix = true,
        bool includeTypeParameters = true,
        bool includeReferenceNullability = true
    )
    {
        var miscOptions = SymbolDisplayMiscellaneousOptions.ExpandNullable;
        if (includeReferenceNullability)
            miscOptions |= SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier;

        return symbol.ToDisplayString(
            new SymbolDisplayFormat(
                withGlobalPrefix
                  ? SymbolDisplayGlobalNamespaceStyle.Included
                  : SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                includeTypeParameters
                  ? SymbolDisplayGenericsOptions.IncludeTypeParameters
                  : SymbolDisplayGenericsOptions.None,
                miscellaneousOptions: miscOptions
            )
        );
    }

    public static string GetFieldSymbolFullName(this IFieldSymbol symbol)
    {
        return $"global::{symbol.ToDisplayString()}";
    }
}
