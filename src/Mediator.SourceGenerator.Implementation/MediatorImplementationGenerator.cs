using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Runtime;
using System.Text;

namespace Mediator.SourceGenerator;

internal static class MediatorImplementationGenerator
{
    internal static void Generate(
        CompilationModel compilationModel,
        Action<string, SourceText> addSource,
        Action<Exception> reportError
    )
    {
        if (compilationModel.HasErrors)
        {
            GenerateFallback(compilationModel, addSource, reportError);
            return;
        }

        try
        {
            var file = @"resources/Mediator.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            //var output = template.Render(compilationAnalyzer, member => member.Name);

            var model = compilationModel;
            var scriptObject = new ScriptObject();
            MemberRenamerDelegate memberRenamer = member => member.Name;
            scriptObject.Import(model, renamer: memberRenamer);

            var context = new TemplateContext();
            context.MemberRenamer = memberRenamer;
            context.PushGlobal(scriptObject);
            context.LoopLimit = 0;
            context.LoopLimitQueryable = 0;
            var output = template.Render(context);

            addSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            reportError(ex);
        }
    }

    private static void GenerateFallback(
        CompilationModel compilationModel,
        Action<string, SourceText> addSource,
        Action<Exception> reportError
    )
    {
        try
        {
            var file = "resources/MediatorFallback.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            var output = template.Render(compilationModel, member => member.Name);

            addSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            reportError(ex);
        }
    }
}
