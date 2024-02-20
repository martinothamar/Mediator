using System.Collections.Immutable;

namespace Mediator.SourceGenerator;

[Generator]
public sealed class IncrementalMediatorGenerator : IIncrementalGenerator
{
    internal CompilationAnalyzer? CompilationAnalyzer;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(
            static context =>
            {
                var generatorVersion = Versioning.GetVersion();

                MediatorOptionsGenerator.Generate(context.AddSource, generatorVersion);
            }
        );

        var compilationProvider = context.CompilationProvider;
        var addMediatorCalls = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => SyntaxReceiver.ShouldVisit(s, out var _),
            transform: static (ctx, _) => (InvocationExpressionSyntax)ctx.Node
        );

        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<InvocationExpressionSyntax> AddMediatorCalls)> source =
            compilationProvider.Combine(addMediatorCalls.Collect());

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
                MediatorImplementationGenerator.Generate(source, context.AddSource, reportDiagnostic);
            }
        );
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

        CompilationAnalyzer = compilationAnalyzer;
        return (diagnostics.ToImmutableEquatableArray(), compilationAnalyzer.ToModel());
    }
}
