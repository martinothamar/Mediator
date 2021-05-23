using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;

namespace Mediator.SourceGenerator
{
    [Generator]
    public sealed partial class MediatorGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var debugOptionExists = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.Mediator_AttachDebugger", out _);

            if (debugOptionExists && !System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Launch();

            //System.Diagnostics.Debugger.Launch();

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
            GenerateOptionsAttribute(in context);

            var compilationAnalyzer = new CompilationAnalyzer(in context, typeof(MediatorGenerator));
            compilationAnalyzer.Analyze(context.CancellationToken);

            if (compilationAnalyzer.HasErrors)
                return;

            var mediatorImplementationGenerator = new MediatorImplementationGenerator();
            mediatorImplementationGenerator.Generate(in context, compilationAnalyzer);
        }

        private void GenerateOptionsAttribute(in GeneratorExecutionContext context)
        {
            var attributeSource = EmbeddedResource.GetContent(@"resources/MediatorOptionsAttribute.cs");
            context.AddSource("MediatorOptionsAttribute.g.cs", SourceText.From(attributeSource, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
