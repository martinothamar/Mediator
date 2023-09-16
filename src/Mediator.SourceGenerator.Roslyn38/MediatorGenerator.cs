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
            context.CancellationToken
        );

        var compilationAnalyzer = new CompilationAnalyzer(in analyzerContext);

        compilationAnalyzer.Initialize();
        compilationAnalyzer.Analyze();

        CompilationAnalyzer = compilationAnalyzer;

        var model = compilationAnalyzer.ToModel();

        var report = context.ReportDiagnostic;
        var reportDiagnostic = (Exception exception) => report.ReportGenericError(exception);
        MediatorImplementationGenerator.Generate(model, context.AddSource, reportDiagnostic);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
