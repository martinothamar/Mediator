using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

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

    public static bool SatisfiesConstraints(
        this ISymbol symbol,
        IReadOnlyList<ITypeSymbol> typeArguments,
        Compilation compilation
    )
    {
        // Taken from https://github.com/dotnet/roslyn/issues/46998
        // and https://github.com/YairHalberstadt/stronginject/blob/f877bde7da049fd0c6952de7632b6b7aa72b0788/StrongInject.Generator/GenericResolutionHelpers.cs#L138
        var typeParameters = symbol.TypeParameters();
        for (int i = 0; i < typeParameters.Length; i++)
        {
            var typeParameter = typeParameters[i];
            var typeArgument = typeArguments[i];

            if (typeArgument.IsPointerOrFunctionPointer() || typeArgument.IsRefLikeType)
            {
                return false;
            }

            if (
                typeParameter.HasReferenceTypeConstraint && !typeArgument.IsReferenceType
                || typeParameter.HasValueTypeConstraint && !typeArgument.IsNonNullableValueType()
                || typeParameter.HasUnmanagedTypeConstraint
                    && !(typeArgument.IsUnmanagedType && typeArgument.IsNonNullableValueType())
                || typeParameter.HasConstructorConstraint && !SatisfiesConstructorConstraint(typeArgument)
            )
            {
                return false;
            }

            foreach (var typeConstraint in typeParameter.ConstraintTypes)
            {
                var substitutedConstraintType = SubstituteType(compilation, typeConstraint, symbol, typeArguments);
                var conversion = compilation.ClassifyConversion(typeArgument, substitutedConstraintType);
                if (
                    typeArgument.IsNullableType()
                    || conversion
                        is not ({ IsIdentity: true } or { IsImplicit: true, IsReference: true } or { IsBoxing: true })
                )
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static bool SatisfiesConstructorConstraint(ITypeSymbol typeArgument)
    {
        switch (typeArgument.TypeKind)
        {
            case TypeKind.Struct:
            case TypeKind.Enum:
            case TypeKind.Dynamic:
                return true;

            case TypeKind.Class:
                return HasPublicParameterlessConstructor((INamedTypeSymbol)typeArgument) && !typeArgument.IsAbstract;

            case TypeKind.TypeParameter:
            {
                var typeParameter = (ITypeParameterSymbol)typeArgument;
                return typeParameter.HasConstructorConstraint || typeParameter.IsValueType;
            }

            default:
                return false;
        }
    }

    private static bool HasPublicParameterlessConstructor(INamedTypeSymbol type)
    {
        foreach (var constructor in type.InstanceConstructors)
        {
            if (constructor.Parameters.Length == 0)
            {
                return constructor.DeclaredAccessibility == Accessibility.Public;
            }
        }
        return false;
    }

    private static ITypeSymbol SubstituteType(
        Compilation compilation,
        ITypeSymbol type,
        ISymbol symbol,
        IReadOnlyList<ITypeSymbol> typeArguments
    )
    {
        return Visit(type);

        ITypeSymbol Visit(ITypeSymbol type)
        {
            switch (type)
            {
                case ITypeParameterSymbol typeParameterSymbol:
                    return SymbolEqualityComparer.Default.Equals(typeParameterSymbol.DeclaringSymbol(), symbol)
                        ? typeArguments[typeParameterSymbol.Ordinal]
                        : type;
                case IArrayTypeSymbol { ElementType: var elementType, Rank: var rank } arrayTypeSymbol:
                    var visitedElementType = Visit(elementType);
                    return ReferenceEquals(elementType, visitedElementType)
                        ? arrayTypeSymbol
                        : compilation.CreateArrayTypeSymbol(visitedElementType, rank);
                case INamedTypeSymbol
                {
                    OriginalDefinition: var originalDefinition,
                    TypeArguments: var typeArguments
                } namedTypeSymbol:
                    var visitedTypeArguments = new ITypeSymbol[typeArguments.Length];
                    var anyChanged = false;
                    for (var i = 0; i < typeArguments.Length; i++)
                    {
                        var typeArgument = typeArguments[i];
                        var visited = Visit(typeArgument);
                        if (!ReferenceEquals(visited, typeArgument))
                            anyChanged = true;
                        visitedTypeArguments[i] = visited;
                    }
                    return anyChanged ? originalDefinition.Construct(visitedTypeArguments) : namedTypeSymbol;
                default:
                    return type;
            }
        }
    }

    public static ImmutableArray<ITypeParameterSymbol> TypeParameters(this ISymbol symbol)
    {
        return symbol switch
        {
            IMethodSymbol { TypeParameters: var tps } => tps,
            INamedTypeSymbol { TypeParameters: var tps } => tps,
            _ => throw new InvalidOperationException($"Symbol of kind '{symbol.Kind}' cannot have type parameters"),
        };
    }

    public static bool IsPointerOrFunctionPointer(this ITypeSymbol type)
    {
        switch (type.TypeKind)
        {
            case TypeKind.Pointer:
            case TypeKind.FunctionPointer:
                return true;

            default:
                return false;
        }
    }

    public static bool IsNonNullableValueType(this ITypeSymbol typeArgument)
    {
        if (!typeArgument.IsValueType)
        {
            return false;
        }

        return !IsNullableTypeOrTypeParameter(typeArgument);
    }

    public static bool IsNullableTypeOrTypeParameter(this ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.TypeParameter)
        {
            var constraintTypes = ((ITypeParameterSymbol)type).ConstraintTypes;
            foreach (var constraintType in constraintTypes)
            {
                if (constraintType.IsNullableTypeOrTypeParameter())
                {
                    return true;
                }
            }
            return false;
        }

        return type.IsNullableType();
    }

    public static bool IsNullableType(this ITypeSymbol type)
    {
        return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    public static ISymbol? DeclaringSymbol(this ITypeParameterSymbol symbol)
    {
        return (ISymbol?)symbol.DeclaringMethod ?? symbol.DeclaringType;
    }
}
