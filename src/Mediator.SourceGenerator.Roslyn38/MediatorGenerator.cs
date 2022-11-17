namespace Mediator.SourceGenerator;

[Generator]
public sealed class MediatorGenerator : ISourceGenerator
{
    internal CompilationAnalyzer? CompilationAnalyzer;

    public void Execute(GeneratorExecutionContext context)
    {
        var debugOptionExists = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
            "build_property.Mediator_AttachDebugger",
            out _
        );

        if (debugOptionExists && !System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Launch();

        ExecuteInternal(in context);
    }

    private void ExecuteInternal(in GeneratorExecutionContext context)
    {
        var generatorVersion = Versioning.GetVersion();

        MediatorOptionsGenerator.Generate(context.AddSource, generatorVersion);

        var analyzerContext = new CompilationAnalyzerContext(
            context.Compilation,
            (context.SyntaxReceiver as SyntaxReceiver)?.AddMediatorCalls,
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

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
