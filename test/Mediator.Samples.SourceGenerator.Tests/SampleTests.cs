using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

public class SampleTests
{
    private static VisualStudioInstance? _instance;

    private static async Task Test(string samplesRelativeProjectPath)
    {
        _instance ??= MSBuildLocator.RegisterDefaults();

        using var workspace = MSBuildWorkspace.Create();

        var diagnostics = new List<WorkspaceDiagnostic>();
        workspace.WorkspaceFailed += (sender, args) => diagnostics.Add(args.Diagnostic);
        workspace.LoadMetadataForReferencedProjects = true;

        var root = "../../../../../samples/";
        var projectPath = Path.Combine(root, samplesRelativeProjectPath);

        var project = await workspace.OpenProjectAsync(projectPath);

        project.MetadataReferences.Should().NotBeEmpty();
        project.Should().NotBeNull();
        var parseOptions = project.ParseOptions as CSharpParseOptions;
        parseOptions.Should().NotBeNull();
        Assert.NotNull(parseOptions);
        var compilation = await project.GetCompilationAsync();
        compilation.Should().NotBeNull();
        Assert.NotNull(compilation);

        diagnostics.Where(d => d.Kind == WorkspaceDiagnosticKind.Failure).Should().BeEmpty();

        var generator = new IncrementalMediatorGenerator().AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var newDiagnostics);
        driver.Should().NotBeNull();

        var result = driver.GetRunResult();
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Length.Should().Be(3);
    }

    public async Task Test_ASPNET_Core_CleanArchitecture_Sample()
    {
        await Test("ASPNET_Core_CleanArchitecture/AspNetCoreSample.Api/AspNetCoreSample.Api.csproj");
    }

    public async Task Test_InternalMessages_Sample()
    {
        await Test("InternalMessages/InternalMessages.Api/InternalMessages.Api.csproj");
    }

    public async Task Test_ASPNET_Core_Indirect_Sample()
    {
        await Test("ASPNET_Core_Indirect/AspNetCoreIndirect.Application/AspNetCoreIndirect.Application.csproj");
    }
}
