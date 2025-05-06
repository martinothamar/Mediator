using System.Text;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace Mediator.SourceGenerator;

internal static class MediatorOptionsGenerator
{
    public static void Generate(Action<string, SourceText> addSource, CompilationModel model)
    {
        GenerateOptions(addSource, model);
        GenerateOptionsAttribute(addSource, model);
        GenerateAssemblyReference(addSource, model);
    }

    private static void GenerateOptions(Action<string, SourceText> addSource, CompilationModel model)
    {
        var file = @"resources/MediatorOptions.sbn-cs";
        var template = Template.Parse(EmbeddedResource.GetContent(file), file);
        var output = template.Render(model, member => member.Name);
        addSource("MediatorOptions.g.cs", SourceText.From(output, Encoding.UTF8));
    }

    private static void GenerateOptionsAttribute(Action<string, SourceText> addSource, CompilationModel model)
    {
        var file = @"resources/MediatorOptionsAttribute.sbn-cs";
        var template = Template.Parse(EmbeddedResource.GetContent(file), file);
        var output = template.Render(model, member => member.Name);
        addSource("MediatorOptionsAttribute.g.cs", SourceText.From(output, Encoding.UTF8));
    }

    private static void GenerateAssemblyReference(Action<string, SourceText> addSource, CompilationModel model)
    {
        var file = @"resources/AssemblyReference.sbn-cs";
        var template = Template.Parse(EmbeddedResource.GetContent(file), file);
        var output = template.Render(model, member => member.Name);
        addSource("AssemblyReference.g.cs", SourceText.From(output, Encoding.UTF8));
    }
}
