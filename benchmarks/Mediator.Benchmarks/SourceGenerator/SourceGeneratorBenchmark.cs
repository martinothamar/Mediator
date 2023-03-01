using Mediator.SourceGenerator;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediator.Benchmarks.SourceGenerator;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[InProcess]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[EtwProfiler]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class SourceGeneratorBenchmark
{
    private Compilation _compilation;

    private CSharpGeneratorDriver _driver;
    private MSBuildWorkspace _workspace;

    [GlobalSetup]
    public void Setup()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        if (currentDirectory.Contains("bin", StringComparison.OrdinalIgnoreCase))
            currentDirectory = currentDirectory.Split("bin")[0];
        ConsoleLogger.Default.WriteLine("Starting!!! at " + currentDirectory);
        _workspace = MSBuildWorkspace.Create();
        _workspace.WorkspaceFailed += (sender, args) =>
        {
            ConsoleLogger.Default.WriteLineError("-------------------------");
            ConsoleLogger.Default.WriteLineError(args.Diagnostic.ToString());
            ConsoleLogger.Default.WriteLineError("-------------------------");
        };

        var projectFile = Path.Combine(
            currentDirectory,
            "../../samples/ASPNET_Core_CleanArchitecture/AspNetCoreSample.Api/AspNetCoreSample.Api.csproj"
        );
        if (!File.Exists(projectFile))
            throw new Exception("Project doesnt exist");
        else
            ConsoleLogger.Default.WriteLine("Project exists!");

        Project project = null;
        try
        {
            ConsoleLogger.Default.WriteLine("Loading project!!!");
            ConsoleLogger.Default.WriteLine("");
            project = _workspace.OpenProjectAsync(projectFile).GetAwaiter().GetResult();
            ConsoleLogger.Default.WriteLine("");
            ConsoleLogger.Default.WriteLine("Loaded project!!!");
        }
        catch (Exception ex)
        {
            ConsoleLogger.Default.WriteError(ex.Message);
            throw;
        }

        _compilation = project.GetCompilationAsync().GetAwaiter().GetResult();
        if (_compilation is null)
            throw new InvalidOperationException("Compilation returned null");

        var generator = GeneratorExtensions.AsSourceGenerator(new IncrementalMediatorGenerator());

        _driver = CSharpGeneratorDriver.Create(
            new[] { generator },
            parseOptions: (CSharpParseOptions)project.ParseOptions!
        );
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _workspace.Dispose();
    }

    [Benchmark]
    public GeneratorDriver Compile()
    {
        return _driver.RunGeneratorsAndUpdateCompilation(_compilation, out var newCompilation, out var newDiagnostics);
    }
}
