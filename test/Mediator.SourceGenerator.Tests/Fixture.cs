using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Mediator.SourceGenerator.Tests;

public static class Fixture
{
    public static Compilation CreateLibrary(params string[] source)
    {
        var references = new List<MetadataReference>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            if (!assembly.IsDynamic)
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        var compilation = CSharpCompilation.Create(
            "compilation",
            source.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray(),
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary
            )
        );

        return compilation;
    }

    public static Task<string> SourceFromResourceFile(string file) => File.ReadAllTextAsync(Path.Combine("resources", file));
}
