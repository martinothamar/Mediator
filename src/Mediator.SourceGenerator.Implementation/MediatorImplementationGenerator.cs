using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Text;

namespace Mediator.SourceGenerator;

internal sealed partial class MediatorImplementationGenerator
{
    internal void Generate(CompilationAnalyzer compilationAnalyzer)
    {
        var compilation = compilationAnalyzer.Compilation;

        var file = @"resources/Mediator.sbn-cs";
        var template = Template.Parse(EmbeddedResource.GetContent(file), file);
        var output = template.Render(compilationAnalyzer, member => member.Name);

        //System.Diagnostics.Debugger.Launch();

        compilationAnalyzer.Context.AddSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
    }
}
