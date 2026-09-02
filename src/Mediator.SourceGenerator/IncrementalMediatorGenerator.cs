using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations;

namespace Mediator.SourceGenerator;

[Generator]
public sealed class IncrementalMediatorGenerator : IIncrementalGenerator
{
    internal CompilationAnalyzer? CompilationAnalyzer;
    internal CompilationModel? CompilationModel;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider;
        var addMediatorCalls = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => SyntaxReceiver.ShouldVisit(s, out var _),
            transform: static (ctx, _) => (InvocationExpressionSyntax)ctx.Node
        );
        var notificationSendDiagnostics = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node
                        is InvocationExpressionSyntax
                        {
                            Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Send" },
                            ArgumentList.Arguments.Count: > 0
                        },
                transform: static (context, cancellationToken) => AnalyzeNotificationSend(context, cancellationToken)
            )
            .Where(static diagnostic => diagnostic is not null);

        context.RegisterSourceOutput(
            notificationSendDiagnostics,
            static (context, diagnostic) => context.ReportDiagnostic(diagnostic!)
        );

        IncrementalValueProvider<(
            Compilation Compilation,
            ImmutableArray<InvocationExpressionSyntax> AddMediatorCalls
        )> source = compilationProvider.Combine(addMediatorCalls.Collect());

        var parsed = source.Select((x, token) => Parse(x.Compilation, x.AddMediatorCalls, token));
        var errors = parsed
            .Select((x, _) => x.Diagnostics)
            .WithTrackingName(MediatorGeneratorStepName.ReportDiagnostics);
        context.RegisterSourceOutput(
            errors,
            (context, errors) =>
            {
                foreach (var error in errors)
                {
                    context.ReportDiagnostic(error);
                }
            }
        );

        var model = parsed.Select((x, _) => x.Model).WithTrackingName(MediatorGeneratorStepName.BuildMediator);

        context.RegisterSourceOutput(
            model,
            (context, source) =>
            {
                var report = context.ReportDiagnostic;
                var reportDiagnostic = (Exception exception) => report.ReportGenericError(exception);

                MediatorOptionsGenerator.Generate(context.AddSource, source);
                MediatorImplementationGenerator.Generate(source, context.AddSource, reportDiagnostic);
            }
        );
    }

    private static Diagnostic? AnalyzeNotificationSend(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationOperation operation)
            return null;

        var method = operation.TargetMethod;

        var compilation = semanticModel.Compilation;
        var senderType = compilation.GetTypeByMetadataName("Mediator.ISender");
        var notificationType = compilation.GetTypeByMetadataName("Mediator.INotification");
        if (senderType is null || notificationType is null)
            return null;

        if (!IsMediatorSendMethod(method, senderType))
            return null;

        var messageArgument = operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0);
        var argumentValue = messageArgument?.Value;
        while (argumentValue is IConversionOperation { IsImplicit: true } conversion)
            argumentValue = conversion.Operand;

        var argumentType = argumentValue?.Type;
        if (argumentType is null || argumentType.TypeKind == TypeKind.Error)
            return null;

        if (!IsNotificationType(argumentType, notificationType))
            return null;

        return Diagnostics.CreateNotificationPassedToSend(argumentValue!.Syntax.GetLocation());
    }

    private static bool IsNotificationType(ITypeSymbol type, INamedTypeSymbol notificationType) =>
        IsNotificationType(type, notificationType, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default));

    private static bool IsNotificationType(
        ITypeSymbol type,
        INamedTypeSymbol notificationType,
        HashSet<ITypeSymbol> visitedTypes
    )
    {
        if (!visitedTypes.Add(type))
            return false;

        if (SymbolEqualityComparer.Default.Equals(type, notificationType))
            return true;

        if (
            type is INamedTypeSymbol namedType
            && namedType.AllInterfaces.Contains(notificationType, SymbolEqualityComparer.Default)
        )
        {
            return true;
        }

        return type is ITypeParameterSymbol typeParameter
            && typeParameter.ConstraintTypes.Any(constraintType =>
                IsNotificationType(constraintType, notificationType, visitedTypes)
            );
    }

    private static bool IsMediatorSendMethod(IMethodSymbol method, INamedTypeSymbol senderType)
    {
        var containingType = method.ContainingType;
        if (
            !SymbolEqualityComparer.Default.Equals(containingType, senderType)
            && !containingType.AllInterfaces.Contains(senderType, SymbolEqualityComparer.Default)
        )
        {
            return false;
        }

        foreach (var senderMethod in senderType.GetMembers("Send").OfType<IMethodSymbol>())
        {
            if (SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, senderMethod.OriginalDefinition))
                return true;

            if (
                containingType.FindImplementationForInterfaceMember(senderMethod) is IMethodSymbol implementation
                && SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, implementation.OriginalDefinition)
            )
            {
                return true;
            }
        }

        return false;
    }

    private (ImmutableEquatableArray<Diagnostic> Diagnostics, CompilationModel Model) Parse(
        Compilation compilation,
        IReadOnlyList<InvocationExpressionSyntax> addMediatorCalls,
        CancellationToken cancellationToken
    )
    {
        var generatorVersion = Versioning.GetVersion();

        var diagnostics = new List<Diagnostic>();
        var analyzerContext = new CompilationAnalyzerContext(
            compilation,
            addMediatorCalls,
            generatorVersion,
            diagnostics.Add,
            cancellationToken
        );

        var compilationAnalyzer = new CompilationAnalyzer(in analyzerContext);

        compilationAnalyzer.Initialize();
        compilationAnalyzer.Analyze();

        var compilationModel = compilationAnalyzer.ToModel();

        CompilationAnalyzer = compilationAnalyzer;
        CompilationModel = compilationModel;
        return (diagnostics.ToImmutableEquatableArray(), compilationModel);
    }
}
