namespace Mediator.SourceGenerator;

[Generator]
public sealed class MediatorGenerator : ISourceGenerator
{
    internal CompilationAnalyzer? CompilationAnalyzer { get; private set; }

    public void Execute(GeneratorExecutionContext context)
    {
        var debugOptionExists = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.Mediator_AttachDebugger", out _);

        if (debugOptionExists && !System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Launch();

        try
        {
            ExecuteInternal(in context);
        }
        catch (Exception exception)
        {
            context.ReportGenericError(exception);

            throw;
        }
    }

    private void ExecuteInternal(in GeneratorExecutionContext context)
    {
        var generatorVersion = Versioning.GetVersion();

        CompilationAnalyzer = new CompilationAnalyzer(in context, generatorVersion);
        CompilationAnalyzer.Analyze(context.CancellationToken);

        if (CompilationAnalyzer.HasErrors)
            return;

        var mediatorImplementationGenerator = new MediatorImplementationGenerator();
        mediatorImplementationGenerator.Generate(in context, CompilationAnalyzer);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(context =>
        {
            var generatorVersion = Versioning.GetVersion();

            MediatorOptionsGenerator.Generate(in context, generatorVersion);
        });
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

}
