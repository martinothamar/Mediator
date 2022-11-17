using System.Collections.Immutable;

namespace Mediator.SourceGenerator;

[Generator]
public sealed class IncrementalMediatorGenerator : IIncrementalGenerator
{
    internal CompilationAnalyzer? CompilationAnalyzer;

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
            context.AddSource,
            context.CancellationToken
        );

        var compilationAnalyzer = new CompilationAnalyzer(in analyzerContext);

        compilationAnalyzer.Initialize();
        compilationAnalyzer.Analyze();

        CompilationAnalyzer = compilationAnalyzer;

        var mediatorImplementationGenerator = new MediatorImplementationGenerator();
        mediatorImplementationGenerator.Generate(compilationAnalyzer);
    }

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

        context.RegisterSourceOutput(
            source,
            (context, source) =>
            {
                ExecuteInternal(in context, source.Compilation, source.AddMediatorCalls);
            }
        );
    }
}
