using Mediator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediator.SourceGenerator
{
    [Generator]
    public sealed class ResultTypeGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;

            //System.Diagnostics.Debugger.Launch();

            var resultModels = new List<TemplatingModel>();

            var resultTypes = new[]
            {
                compilation.GetTypeByMetadataName($"{Constants.MediatorLib}.IResult`2")!,
            };

            foreach (var tree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);

                foreach (var node in tree.GetRoot().DescendantNodesAndSelf().OfType<StructDeclarationSyntax>())
                {
                    var structSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(node)!;

                    var resultInterfaceSymbol = resultTypes.Select(r =>
                    {
                        if (structSymbol.AllInterfaces.Length == 0)
                            return null;

                        var candidateInterface = structSymbol.AllInterfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, r));
                        if (candidateInterface is null)
                            return null;

                        var constructedInterface = r.Construct(candidateInterface.TypeArguments.ToArray());
                        return compilation.HasImplicitConversion(structSymbol, constructedInterface) ? constructedInterface : null;
                    }).SingleOrDefault(i => i is not null);

                    if (resultInterfaceSymbol is null)
                        continue;

                    resultModels.Add(new TemplatingModel(structSymbol, resultInterfaceSymbol));
                }
            }

            var file = @"resources/Result.sbn-cs";
            var template = Template.Parse(EmbeddedResource.GetContent(file), file);

            foreach (var resultModel in resultModels)
            {
                var output = template.Render(resultModel, member => member.Name);

                //System.Diagnostics.Debugger.Launch();

                context.AddSource($"Result_{resultModel.Name}.g.cs", SourceText.From(output, Encoding.UTF8));
            }
        }

        private sealed class TemplatingModel
        {
            public readonly INamedTypeSymbol Concrete;
            public readonly INamedTypeSymbol Interface;

            public string Namespace =>
                Concrete.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

            public string InterfaceName =>
                Interface.GetTypeSymbolFullName();

            public string ValueTypeName =>
                Interface.TypeArguments[0].GetTypeSymbolFullName();

            public string Name => Concrete.Name;

            public IEnumerable<Error> Errors
            {
                get
                {
                    var errorTypeSymbols = Interface.TypeArguments;

                    for (int i = 1; i < errorTypeSymbols.Length; i++)
                    {
                        var errorTypeSymbol = errorTypeSymbols[i];

                        var typeName = errorTypeSymbol.GetTypeSymbolFullName();
                        var tag = i + 1;
                        var fieldName = "_tError" + tag;
                        yield return new Error(typeName, fieldName, tag);
                    }
                }
            }

            public TemplatingModel(INamedTypeSymbol concrete, INamedTypeSymbol @interface)
            {
                Concrete = concrete;
                Interface = @interface;
            }

            public sealed class Error
            {
                public readonly string TypeName;
                public readonly string FieldName;
                public readonly int Tag;

                public Error(string typeName, string fieldName, int tag)
                {
                    TypeName = typeName;
                    FieldName = fieldName;
                    Tag = tag;
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
