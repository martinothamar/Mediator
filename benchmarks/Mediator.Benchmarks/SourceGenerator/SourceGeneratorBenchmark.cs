using Mediator.SourceGenerator;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediator.Benchmarks.SourceGenerator;

[MemoryDiagnoser(true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[InProcess]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[EtwProfiler]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class SourceGeneratorBenchmark
{
    private Compilation _compilation;
    const string SmallPath =
        "../../samples/ASPNET_Core_CleanArchitecture/AspNetCoreSample.Api/AspNetCoreSample.Api.csproj";
    const string LargePath = "../Mediator.Benchmarks.Large/Mediator.Benchmarks.Large.csproj";

    private CSharpGeneratorDriver _driver;
    private MSBuildWorkspace _workspace;

    public void Setup(string path)
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

        var projectFile = Path.Combine(currentDirectory, path);
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

    [GlobalSetup(Target = nameof(Compile))]
    public void SetupSmall() => Setup(SmallPath);

    [Benchmark]
    public GeneratorDriver Compile()
    {
        return _driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }

    [GlobalSetup(Target = nameof(Cached))]
    public void SetupCached()
    {
        Setup(SmallPath);
        _driver = (CSharpGeneratorDriver)_driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }

    [Benchmark]
    public GeneratorDriver Cached()
    {
        return _driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }

    [GlobalSetup(Target = nameof(LargeCompile))]
    public void SetupLarge() => Setup(LargePath);

    [Benchmark]
    public GeneratorDriver LargeCompile()
    {
        return _driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }

    [GlobalSetup(Target = nameof(LargeCached))]
    public void SetupLargeCached()
    {
        Setup(LargePath);
        _driver = (CSharpGeneratorDriver)_driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }

    [Benchmark]
    public GeneratorDriver LargeCached()
    {
        return _driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }
}
