using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Linq;
using System.Text;

namespace Mediator.SourceGenerator
{
    internal sealed partial class MediatorImplementationGenerator
    {
        internal void Generate(in GeneratorExecutionContext context, in MediatorGenerationContext mediatorContext)
        {
            var compilation = context.Compilation;

            var handlers = mediatorContext.HandlerMap
                .OrderBy(h => h.Key.Name)
                .Select(h => new TemplatingModel.Handler(h.Key, h.Value, compilation))
                .ToArray();

            var handlerTypes = mediatorContext.HandlerTypes
                .Select(ht =>
                {
                    var messageType = ht.TypeArguments[0].Name.Substring(1);
                    var hasResponse = ht.MetadataName.EndsWith("2", StringComparison.InvariantCulture);
                    return new TemplatingModel.HandlerType(messageType, hasResponse);
                })
                .ToArray();

            var model = new TemplatingModel(mediatorContext.MediatorNamespace, handlers, handlerTypes);

            var file = @"resources/Mediator.sbntxt";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);
            var output = template.Render(model, member => member.Name);

            context.AddSource("Mediator.g.cs", SourceText.From(output, Encoding.UTF8));
        }
    }
}
