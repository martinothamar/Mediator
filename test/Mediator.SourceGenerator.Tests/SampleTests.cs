using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using VerifyXunit;

namespace Mediator.SourceGenerator.Tests;

public sealed class SampleTests
{
    private static async Task Test(string samplesRelativeProjectPath)
    {
        using var workspace = MSBuildWorkspace.Create();

        var diagnostics = new List<WorkspaceDiagnostic>();
        workspace.WorkspaceFailed += (sender, args) => diagnostics.Add(args.Diagnostic);
        workspace.LoadMetadataForReferencedProjects = true;

        var slnDir = Fixture.GetSolutionDirectoryInfo();
        var samplesDir = Path.Combine(slnDir.FullName, "samples");
        var projectPath = Path.Combine(samplesDir, samplesRelativeProjectPath);

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

        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var newDiagnostics);
        driver.Should().NotBeNull();

        var result = driver.GetRunResult();
        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Length.Should().Be(3);

        await Verifier.Verify(driver);
    }

    [Fact]
    public async Task Test_ASPNET_Core_CleanArchitecture_Sample() =>
        await Test("apps/ASPNET_Core_CleanArchitecture/AspNetCoreSample.Api/AspNetCoreSample.Api.csproj");

    [Fact]
    public async Task Test_InternalMessages_Sample() =>
        await Test("apps/InternalMessages/InternalMessages.Api/InternalMessages.Api.csproj");

    [Fact]
    public async Task Test_ASPNET_Core_Indirect_Sample() =>
        await Test("apps/ASPNET_Core_Indirect/AspNetCoreIndirect.Application/AspNetCoreIndirect.Application.csproj");

    [Fact]
    public async Task Test_Console() => await Test("basic/Console/Console.csproj");

    [Fact]
    public async Task Test_ConsoleAOT() => await Test("basic/ConsoleAOT/ConsoleAOT.csproj");

    [Fact]
    public async Task Test_Notifications() => await Test("basic/Notifications/Notifications.csproj");

    [Fact]
    public async Task Test_Streaming() => await Test("basic/Streaming/Streaming.csproj");

    [Fact]
    public async Task Test_Showcase() => await Test("Showcase/Showcase.csproj");
}
