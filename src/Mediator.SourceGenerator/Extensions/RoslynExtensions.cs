using Microsoft.CodeAnalysis;

namespace Mediator.SourceGenerator.Extensions
{
    public static class RoslynExtensions
    {
        public static string GetTypeSymbolFullName(this ITypeSymbol symbol)
        {
            return symbol.ToDisplayString(new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Included,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
            ));
        }
    }
}
