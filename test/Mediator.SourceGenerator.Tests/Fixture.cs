using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Mediator.SourceGenerator.Tests;

public static class Fixture
{
    public static Compilation CreateCompilation(params string[] source)
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

        return CSharpCompilation.Create("compilation",
            source.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray(),
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static Task<string> SourceFromResourceFile(string file) => File.ReadAllTextAsync(Path.Combine("resources", file));
}
