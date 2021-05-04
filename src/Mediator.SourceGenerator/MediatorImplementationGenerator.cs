using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Text;

namespace Mediator.SourceGenerator
{
    internal sealed partial class MediatorImplementationGenerator
    {
        internal void Generate(in GeneratorExecutionContext context, CompilationAnalyzer compilationAnalyzer)
        {
            var compilation = context.Compilation;

            var model = new TemplatingModel(
                compilationAnalyzer.MediatorNamespace,
                compilationAnalyzer.ConcreteHandlers,
                compilationAnalyzer.BaseHandlers
            );

            //System.Diagnostics.Debugger.Launch();

            var file = @"resources/Mediator.sbntxt";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            var output = template.Render(model, member => member.Name);

            context.AddSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
    }
}
