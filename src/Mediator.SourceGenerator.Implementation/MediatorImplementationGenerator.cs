using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Runtime;
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
            string? file = @"resources/Mediator.sbn-cs";
            Template? template = Template.Parse(EmbeddedResource.GetContent(file), file);

            CompilationAnalyzer model = compilationAnalyzer;
            ScriptObject scriptObject = new();
            MemberRenamerDelegate memberRenamer = member => member.Name;
            if (model is not null)
                scriptObject.Import(model, renamer: memberRenamer);

            TemplateContext context = new() { MemberRenamer = memberRenamer };
            context.PushGlobal(scriptObject);
            context.LoopLimit = 0;
            context.LoopLimitQueryable = 0;
            string? output = template.Render(context);

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
            string? file = "resources/MediatorFallback.sbn-cs";
            Template? template = Template.Parse(EmbeddedResource.GetContent(file), file);
            string? output = template.Render(compilationAnalyzer, member => member.Name);

            compilationAnalyzer.Context.AddSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            compilationAnalyzer.Context.ReportGenericError(ex);
        }
    }
}
