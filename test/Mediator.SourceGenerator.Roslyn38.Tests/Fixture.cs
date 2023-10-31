using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

public static class Fixture
{
    public static readonly Assembly[] ImportantAssemblies = new[]
    {
        typeof(object).Assembly,
        typeof(IMessage).Assembly,
        typeof(ServiceLifetime).Assembly,
        typeof(ServiceProvider).Assembly,
        typeof(MulticastDelegate).Assembly
    };

    public static Assembly[] AssemblyReferencesForCodegen =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .Concat(ImportantAssemblies)
            .Distinct()
            .Where(a => !a.IsDynamic)
            .ToArray();

    public static Compilation CreateLibrary(params string[] source)
    {
        var references = new List<MetadataReference>();
        var assemblies = AssemblyReferencesForCodegen;
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
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return compilation;
    }

    public static Task VerifyGenerator(string source)
    {
        var compilation = CreateLibrary(source);

        var generator = new MediatorGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var ranDriver = driver.RunGenerators(compilation);
        var verify = VerifyXunit.Verifier.Verify(ranDriver);

        return verify.ToTask();
    }

    public static Task<string> SourceFromResourceFile(string file) =>
        File.ReadAllTextAsync(Path.Combine("resources", file));
}
