using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
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
            var generatorAssembly = GetType().Assembly;
            var generatorVersion = generatorAssembly.GetName().Version.ToString();

            GenerateOptionsAttribute(in context, generatorVersion);

            var compilationAnalyzer = new CompilationAnalyzer(in context, generatorVersion);
            compilationAnalyzer.Analyze(context.CancellationToken);

            if (compilationAnalyzer.HasErrors)
                return;

            var mediatorImplementationGenerator = new MediatorImplementationGenerator();
            mediatorImplementationGenerator.Generate(in context, compilationAnalyzer);
        }

        private void GenerateOptionsAttribute(in GeneratorExecutionContext context, string generatorVersion)
        {
            var model = new { GeneratorVersion = generatorVersion };

            var file = @"resources/MediatorOptionsAttribute.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            var output = template.Render(model, member => member.Name);
            context.AddSource("MediatorOptionsAttribute.g.cs", SourceText.From(output, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
