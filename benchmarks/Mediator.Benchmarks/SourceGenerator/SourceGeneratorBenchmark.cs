using System.Runtime.CompilerServices;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Mediator.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.SourceGenerator;

// Currently broken, wait for release of:
// https://github.com/Buildalyzer/Buildalyzer/pull/319
// Or implement building ourself

[MemoryDiagnoser(true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class SourceGeneratorBenchmark
{
    private static string ProjectDir([CallerFilePath] string thisPath = "") =>
        new DirectoryInfo(Path.GetDirectoryName(thisPath)).Parent.FullName;

    private AdhocWorkspace _workspace;
    private Compilation _compilation;
    private CSharpGeneratorDriver _driver;

    [Params("Small", "Large")]
    public string ProjectType { get; set; }

    [ParamsAllValues]
    public ServiceLifetime ServiceLifetime { get; set; }

    public void Setup()
    {
        var currentDirectory = ProjectDir();
        ConsoleLogger.Default.WriteLine("Starting!!! at " + currentDirectory);
        AnalyzerManager manager = new AnalyzerManager();
        IProjectAnalyzer analyzer = manager.GetProject(Path.Combine(currentDirectory, "Mediator.Benchmarks.csproj"));
        List<string> extraDefineConstants = [];
        switch (ServiceLifetime)
        {
            case ServiceLifetime.Singleton:
                extraDefineConstants.Add("Mediator_Lifetime_Singleton");
                break;
            case ServiceLifetime.Scoped:
                extraDefineConstants.Add("Mediator_Lifetime_Scoped");
                break;
            case ServiceLifetime.Transient:
                extraDefineConstants.Add("Mediator_Lifetime_Transient");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ServiceLifetime), ServiceLifetime, null);
        }

        extraDefineConstants.Add($"Mediator_{ProjectType}_Project");

        analyzer.SetGlobalProperty("ExtraDefineConstants", string.Join(";", extraDefineConstants));

        _workspace = analyzer.GetWorkspace(addProjectReferences: true);
        var project = _workspace.CurrentSolution.Projects.Single(p => p.Name == "Mediator.Benchmarks");
        _compilation = project.GetCompilationAsync().GetAwaiter().GetResult();
        var generator = GeneratorExtensions.AsSourceGenerator(new IncrementalMediatorGenerator());

        _driver = CSharpGeneratorDriver.Create([generator], parseOptions: (CSharpParseOptions)project.ParseOptions!);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _workspace.Dispose();
    }

    [GlobalSetup(Target = nameof(Cold))]
    public void SetupCold() => Setup();

    [GlobalSetup(Target = nameof(Cached))]
    public void SetupCached()
    {
        Setup();
        _driver = (CSharpGeneratorDriver)_driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }

    [Benchmark]
    public GeneratorDriver Cold()
    {
        return _driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }

    [Benchmark]
    public GeneratorDriver Cached()
    {
        return _driver.RunGeneratorsAndUpdateCompilation(_compilation, out _, out _);
    }
}
