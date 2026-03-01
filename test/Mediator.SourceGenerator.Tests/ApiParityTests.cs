using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mediator.SourceGenerator.Tests;

public sealed class ApiParityTests
{
    private const string MediatorNamespace = "ApiParity";

    private const string MediatorCompatibilityStubs = """
        namespace Mediator;

        public enum CachingMode
        {
            Eager = 0,
            Lazy = 1,
        }

        public sealed class MediatorOptions
        {
            public Microsoft.Extensions.DependencyInjection.ServiceLifetime ServiceLifetime { get; set; } =
                Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton;
        }
        """;

    private static readonly SymbolDisplayFormat TypeDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    private static readonly SymbolDisplayFormat MemberDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
        memberOptions: SymbolDisplayMemberOptions.IncludeAccessibility
            | SymbolDisplayMemberOptions.IncludeModifiers
            | SymbolDisplayMemberOptions.IncludeType
            | SymbolDisplayMemberOptions.IncludeContainingType
            | SymbolDisplayMemberOptions.IncludeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType
            | SymbolDisplayParameterOptions.IncludeName
            | SymbolDisplayParameterOptions.IncludeParamsRefOut
            | SymbolDisplayParameterOptions.IncludeExtensionThis
            | SymbolDisplayParameterOptions.IncludeDefaultValue,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void Fallback_Generated_Api_Matches_Main_Template(
        bool generateTypesAsInternal,
        bool enableMetrics,
        bool enableTracing
    )
    {
        var mainSource = RenderMediator(
            CreateModel(
                hasErrors: false,
                generateTypesAsInternal: generateTypesAsInternal,
                enableMetrics: enableMetrics,
                enableTracing: enableTracing
            )
        );
        var fallbackSource = RenderMediator(
            CreateModel(
                hasErrors: true,
                generateTypesAsInternal: generateTypesAsInternal,
                enableMetrics: enableMetrics,
                enableTracing: enableTracing
            )
        );

        var mainCompilation = Fixture.CreateLibrary(MediatorCompatibilityStubs, mainSource);
        var fallbackCompilation = Fixture.CreateLibrary(MediatorCompatibilityStubs, fallbackSource);
        AssertCompilesWithoutErrors(mainCompilation);
        AssertCompilesWithoutErrors(fallbackCompilation);

        AssertPublicApiEqual(
            mainCompilation,
            fallbackCompilation,
            "Microsoft.Extensions.DependencyInjection.MediatorDependencyInjectionExtensions"
        );
        AssertPublicApiEqual(mainCompilation, fallbackCompilation, $"{MediatorNamespace}.Mediator");
    }

    private static CompilationModel CreateModel(
        bool hasErrors,
        bool generateTypesAsInternal,
        bool enableMetrics,
        bool enableTracing
    ) =>
        new(
            requestMessages: ImmutableEquatableArray.Empty<RequestMessageModel>(),
            notificationMessages: ImmutableEquatableArray.Empty<NotificationMessageModel>(),
            notificationMessageHandlers: ImmutableEquatableArray.Empty<NotificationMessageHandlerModel>(),
            requestMessageHandlerWrappers: ImmutableEquatableArray.Empty<RequestMessageHandlerWrapperModel>(),
            notificationPublisherType: new(
                FullName: "global::Mediator.ForeachAwaitPublisher",
                Name: "ForeachAwaitPublisher"
            ),
            pipelineBehaviors: ImmutableEquatableArray.Empty<PipelineBehaviorModel>(),
            hasErrors: hasErrors,
            mediatorNamespace: MediatorNamespace,
            generatorVersion: "1.0.0.0",
            serviceLifetime: "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton",
            serviceLifetimeShort: "Singleton",
            singletonServiceLifetime: "global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton",
            isTestRun: false,
            configuredViaAttribute: false,
            generateTypesAsInternal: generateTypesAsInternal,
            cachingMode: "global::Mediator.CachingMode.Eager",
            cachingModeShort: "Eager",
            enableMetrics: enableMetrics,
            meterName: "Mediator.Parity.Metrics",
            enableTracing: enableTracing,
            activitySourceName: "Mediator.Parity.ActivitySource",
            histogramBuckets: null,
            targetFrameworkIsNet8OrGreater: true,
            targetFrameworkIsNet9OrGreater: true,
            targetHasMeter: true,
            targetHasActivitySource: true
        );

    private static string RenderMediator(CompilationModel model)
    {
        SourceText? sourceText = null;
        Exception? exception = null;

        MediatorImplementationGenerator.Generate(
            model,
            (hintName, source) =>
            {
                if (hintName == "Mediator.g.cs")
                {
                    sourceText = source;
                }
            },
            ex => exception = ex
        );

        Assert.Null(exception);
        Assert.NotNull(sourceText);
        return sourceText!.ToString();
    }

    private static void AssertPublicApiEqual(
        Compilation expectedCompilation,
        Compilation actualCompilation,
        string metadataName
    )
    {
        var expected = GetPublicApiShape(expectedCompilation, metadataName);
        var actual = GetPublicApiShape(actualCompilation, metadataName);

        Assert.Equal(expected, actual);
    }

    private static IReadOnlyList<string> GetPublicApiShape(Compilation compilation, string metadataName)
    {
        var type = compilation.GetTypeByMetadataName(metadataName);
        Assert.NotNull(type);
        var interfaces = type!
            .Interfaces.Select(i => i.ToDisplayString(TypeDisplayFormat))
            .OrderBy(static x => x, StringComparer.Ordinal);

        var shape = new List<string>
        {
            $"TYPE {type.DeclaredAccessibility} {type.TypeKind} {type.ToDisplayString(TypeDisplayFormat)} static={type.IsStatic} abstract={type.IsAbstract} sealed={type.IsSealed}",
            $"INTERFACES [{string.Join(", ", interfaces)}]",
        };

        var members = type.GetMembers()
            .Where(ShouldIncludePublicMember)
            .Select(ToDisplaySignature)
            .OrderBy(static x => x, StringComparer.Ordinal);

        shape.AddRange(members);
        return shape;
    }

    private static bool ShouldIncludePublicMember(ISymbol member)
    {
        if (member.IsImplicitlyDeclared || member.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        return member switch
        {
            IMethodSymbol method => method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor,
            IFieldSymbol => true,
            IPropertySymbol => true,
            IEventSymbol => true,
            _ => false,
        };
    }

    private static string ToDisplaySignature(ISymbol member)
    {
        if (member is IFieldSymbol field && field.HasConstantValue)
        {
            return $"{field.ToDisplayString(MemberDisplayFormat)} = {FormatConstant(field.ConstantValue)}";
        }

        return member.ToDisplayString(MemberDisplayFormat);
    }

    private static string FormatConstant(object? constant) =>
        constant switch
        {
            null => "null",
            string s => $"\"{s}\"",
            char c => $"'{c}'",
            bool b => b ? "true" : "false",
            _ => Convert.ToString(constant, CultureInfo.InvariantCulture) ?? "<null>",
        };

    private static void AssertCompilesWithoutErrors(Compilation compilation)
    {
        var diagnostics = compilation.GetDiagnostics();
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }
}
