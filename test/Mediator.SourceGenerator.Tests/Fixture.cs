using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.SourceGenerator.Tests;

public static class Fixture
{
    public static readonly Assembly[] ImportantAssemblies = new[]
    {
        typeof(object).Assembly,
        typeof(Console).Assembly,
        typeof(IMessage).Assembly,
        typeof(ServiceLifetime).Assembly,
        typeof(ServiceProvider).Assembly,
        typeof(MulticastDelegate).Assembly
    };

    public static Assembly[] AssemblyReferencesForCodegen =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .Concat(ImportantAssemblies)
            .Distinct()
            .Where(a => !a.IsDynamic)
            .ToArray();

    public static DirectoryInfo GetSolutionDirectoryInfo()
    {
        var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assert.NotNull(currentDir);
        var directory = new DirectoryInfo(currentDir);
        for (int i = 0; i < 10 && directory is not null && !directory.GetFiles("Mediator.sln").Any(); i++)
            directory = directory.Parent;
        if (directory is null)
            throw new InvalidOperationException("Could not find solution directory");
        return directory;
    }

    public static CSharpCompilation CreateLibrary(params string[] source) =>
        CreateLibrary(source.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray());

    public static CSharpCompilation CreateLibrary(params SyntaxTree[] source)
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
            source,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return compilation;
    }

    public static async Task<string> SourceFromResourceFile(string file)
    {
        var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assert.NotNull(currentDir);
        var resourcesDir = Path.Combine(currentDir, "resources");

        return await File.ReadAllTextAsync(Path.Combine(resourcesDir, file));
    }
}
