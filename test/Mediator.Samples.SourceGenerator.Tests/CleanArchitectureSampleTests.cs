using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Runtime.Loader;

namespace Mediator.SourceGenerator.Tests;

public class CleanArchitectureSampleTests
{
    static CleanArchitectureSampleTests()
    {
        var instance = MSBuildLocator.RegisterDefaults();
        foreach (var context in AssemblyLoadContext.All)
        {
            context.Resolving += (assemblyLoadContext, assemblyName) =>
            {
                // System.Diagnostics.Debugger.Launch();
                Console.WriteLine("Loading assembly: " + assemblyName.Name);
                var msBuildPath = Path.Combine(instance.MSBuildPath, assemblyName.Name + ".dll");
                var projPath = assemblyName.Name + ".dll";
                if (File.Exists(msBuildPath))
                    return assemblyLoadContext.LoadFromAssemblyPath(msBuildPath);
                else if (File.Exists(projPath))
                    return assemblyLoadContext.LoadFromAssemblyPath(projPath);

                return null;
            };
        }
    }

    public async Task Test_Source_Gen()
    {
        Console.WriteLine("Building workspace...");
        using var workspace = MSBuildWorkspace.Create();

        var diagnostics = new List<WorkspaceDiagnostic>();
        workspace.WorkspaceFailed += (sender, args) => diagnostics.Add(args.Diagnostic);

        var project = await workspace.OpenProjectAsync(
            "../../../../../samples/ASPNET_CleanArchitecture/AspNetSample.Api/AspNetSample.Api.csproj"
        );

        project.MetadataReferences.Should().NotBeEmpty();
        project.Should().NotBeNull();
        project.ParseOptions.Should().BeOfType<CSharpParseOptions>().And.NotBeNull();
        var compilation = await project.GetCompilationAsync();
        compilation.Should().NotBeNull();

        // diagnostics.Should().BeEmpty();

        var generator = GeneratorExtensions.AsSourceGenerator(new IncrementalMediatorGenerator());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { generator },
            parseOptions: (CSharpParseOptions)project.ParseOptions!
        );

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation!, out var newCompilation, out var newDiagnostics);
        driver.Should().NotBeNull();

        var result = driver.GetRunResult();
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Length.Should().Be(3);
    }
}
