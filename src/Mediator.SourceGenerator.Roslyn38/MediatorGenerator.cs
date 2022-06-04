namespace Mediator.SourceGenerator;

[Generator]
public sealed class MediatorGenerator : ISourceGenerator
{
    internal CompilationAnalyzer? CompilationAnalyzer { get; private set; }

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

        CompilationAnalyzer = new CompilationAnalyzer(in analyzerContext);

        CompilationAnalyzer.Initialize();
        CompilationAnalyzer.Analyze();

        var mediatorImplementationGenerator = new MediatorImplementationGenerator();
        mediatorImplementationGenerator.Generate(CompilationAnalyzer);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
