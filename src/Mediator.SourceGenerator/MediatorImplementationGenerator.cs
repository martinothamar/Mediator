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

            var file = @"resources/Mediator.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            var output = template.Render(compilationAnalyzer, member => member.Name);

            //System.Diagnostics.Debugger.Launch();

            context.AddSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
    }
}
