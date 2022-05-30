using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Runtime.Loader;

namespace Mediator.SourceGenerator.Tests;

public class CleanArchitectureSampleTests
{
    // TODO: fix this brittle test setup stuff....
    // public async Task Test_Source_Gen()
    // {
    //     var instance = MSBuildLocator.RegisterDefaults();

    //     Console.WriteLine("Building workspace...");
    //     using var workspace = MSBuildWorkspace.Create();

    //     var diagnostics = new List<WorkspaceDiagnostic>();
    //     workspace.WorkspaceFailed += (sender, args) => diagnostics.Add(args.Diagnostic);

    //     var project = await workspace.OpenProjectAsync(
    //         "../../../../../samples/ASPNET_CleanArchitecture/AspNetSample.Api/AspNetSample.Api.csproj"
    //     );

    //     // project.MetadataReferences.Should().NotBeEmpty();
    //     project.Should().NotBeNull();
    //     project.ParseOptions.Should().BeOfType<CSharpParseOptions>().And.NotBeNull();
    //     var compilation = await project.GetCompilationAsync();
    //     compilation.Should().NotBeNull();

    //     // diagnostics.Should().BeEmpty();

    //     var generator = GeneratorExtensions.AsSourceGenerator(new IncrementalMediatorGenerator());

    //     GeneratorDriver driver = CSharpGeneratorDriver.Create(
    //         new[] { generator },
    //         parseOptions: (CSharpParseOptions)project.ParseOptions!
    //     );

    //     driver = driver.RunGeneratorsAndUpdateCompilation(compilation!, out var newCompilation, out var newDiagnostics);
    //     driver.Should().NotBeNull();

    //     var result = driver.GetRunResult();
    //     result.Diagnostics.Should().BeEmpty();
    //     result.GeneratedTrees.Length.Should().Be(3);
    // }
}
