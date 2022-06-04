using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Text;

namespace Mediator.SourceGenerator;

internal sealed partial class MediatorImplementationGenerator
{
    internal void Generate(CompilationAnalyzer compilationAnalyzer)
    {
        if (compilationAnalyzer.HasErrors)
        {
            GenerateFallback(compilationAnalyzer);
            return;
        }

        try
        {
            var file = @"resources/Mediator.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            var output = template.Render(compilationAnalyzer, member => member.Name);

            compilationAnalyzer.Context.AddSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            compilationAnalyzer.Context.ReportGenericError(ex);
        }
    }

    private void GenerateFallback(CompilationAnalyzer compilationAnalyzer)
    {
        try
        {
            var file = "resources/MediatorFallback.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            var output = template.Render(compilationAnalyzer, member => member.Name);

            compilationAnalyzer.Context.AddSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            compilationAnalyzer.Context.ReportGenericError(ex);
        }
    }
}
