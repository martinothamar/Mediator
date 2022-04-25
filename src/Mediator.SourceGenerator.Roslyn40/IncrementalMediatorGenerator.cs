using System.Collections.Immutable;

namespace Mediator.SourceGenerator;

[Generator]
public sealed class IncrementalMediatorGenerator : IIncrementalGenerator
{
    internal CompilationAnalyzer? CompilationAnalyzer { get; private set; }

    private void ExecuteInternal(
        in SourceProductionContext context,
        Compilation compilation,
        IReadOnlyList<InvocationExpressionSyntax> addMediatorCalls
    )
    {

        var generatorVersion = Versioning.GetVersion();

        var analyzerContext = new CompilationAnalyzerContext(
            compilation,
            addMediatorCalls,
            generatorVersion,
            context.ReportDiagnostic,
            context.AddSource
        );
        CompilationAnalyzer = new CompilationAnalyzer(in analyzerContext);
        if (CompilationAnalyzer.HasErrors)
            return;

        CompilationAnalyzer.Analyze(context.CancellationToken);
        if (CompilationAnalyzer.HasErrors)
            return;

        var mediatorImplementationGenerator = new MediatorImplementationGenerator();
        mediatorImplementationGenerator.Generate(CompilationAnalyzer);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //System.Diagnostics.Debugger.Launch();

        context.RegisterPostInitializationOutput(context =>
        {
            var generatorVersion = Versioning.GetVersion();

            MediatorOptionsGenerator.Generate(context.AddSource, generatorVersion);
        });

        var compilationProvider = context.CompilationProvider;
        var addMediatorCalls = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => SyntaxReceiver.ShouldVisit(s, out var _),
            transform: static (ctx, _) => (InvocationExpressionSyntax)ctx.Node
        );

        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<InvocationExpressionSyntax> AddMediatorCalls)> source =
            compilationProvider.Combine(addMediatorCalls.Collect());

        context.RegisterSourceOutput(source, (context, source) =>
        {
            ExecuteInternal(in context, source.Compilation, source.AddMediatorCalls);
        });
    }
}
