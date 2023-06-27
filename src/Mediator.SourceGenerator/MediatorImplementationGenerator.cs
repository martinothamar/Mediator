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
            var file = @"resources/Mediator.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            //var output = template.Render(compilationAnalyzer, member => member.Name);

            var model = compilationAnalyzer;
            var scriptObject = new ScriptObject();
            MemberRenamerDelegate memberRenamer = member => member.Name;
            if (model is not null)
                scriptObject.Import(model, renamer: memberRenamer);

            var context = new TemplateContext();
            context.MemberRenamer = memberRenamer;
            context.PushGlobal(scriptObject);
            context.LoopLimit = 0;
            context.LoopLimitQueryable = 0;
            var output = template.Render(context);

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
