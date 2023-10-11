using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Text;

namespace Mediator.SourceGenerator;

public static class MediatorOptionsGenerator
{
    public static void Generate(Action<string, SourceText> addSource, string generatorVersion)
    {
        GenerateOptions(addSource, generatorVersion);
        GenerateOptionsAttribute(addSource, generatorVersion);
    }

    private static void GenerateOptions(Action<string, SourceText> addSource, string generatorVersion)
    {
        var model = new { GeneratorVersion = generatorVersion };

        string file = @"resources/MediatorOptions.sbn-cs";
        Template template = Template.Parse(EmbeddedResource.GetContent(file), file);
        string output = template.Render(model, member => member.Name);
        addSource("MediatorOptions.g.cs", SourceText.From(output, Encoding.UTF8));
    }

    private static void GenerateOptionsAttribute(Action<string, SourceText> addSource, string generatorVersion)
    {
        var model = new { GeneratorVersion = generatorVersion };

        string file = @"resources/MediatorOptionsAttribute.sbn-cs";
        Template template = Template.Parse(EmbeddedResource.GetContent(file), file);
        string output = template.Render(model, member => member.Name);
        addSource("MediatorOptionsAttribute.g.cs", SourceText.From(output, Encoding.UTF8));
    }
}
