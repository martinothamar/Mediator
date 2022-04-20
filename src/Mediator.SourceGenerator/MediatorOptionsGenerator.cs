using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Text;

namespace Mediator.SourceGenerator;

public static class MediatorOptionsGenerator
{
    public static void Generate(in GeneratorPostInitializationContext context, string generatorVersion)
    {
        GenerateOptions(in context, generatorVersion);
        GenerateOptionsAttribute(in context, generatorVersion);
    }

    private static void GenerateOptions(in GeneratorPostInitializationContext context, string generatorVersion)
    {
        var model = new { GeneratorVersion = generatorVersion };

        var file = @"resources/MediatorOptions.sbn-cs";
        var template = Template.Parse(EmbeddedResource.GetContent(file), file);
        var output = template.Render(model, member => member.Name);
        context.AddSource("MediatorOptions.g.cs", SourceText.From(output, Encoding.UTF8));
    }

    private static void GenerateOptionsAttribute(in GeneratorPostInitializationContext context, string generatorVersion)
    {
        var model = new { GeneratorVersion = generatorVersion };

        var file = @"resources/MediatorOptionsAttribute.sbn-cs";
        var template = Template.Parse(EmbeddedResource.GetContent(file), file);
        var output = template.Render(model, member => member.Name);
        context.AddSource("MediatorOptionsAttribute.g.cs", SourceText.From(output, Encoding.UTF8));
    }
}
