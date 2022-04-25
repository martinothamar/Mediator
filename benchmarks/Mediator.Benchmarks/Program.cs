using BenchmarkDotNet.Running;
using Mediator.SourceGenerator;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Diagnostics;

ConsoleLogger.Default.WriteLine($"Running with lifetime: Impl={Mediator.Mediator.ServiceLifetime}");
ConsoleLogger.Default.WriteLine();

MSBuildLocator.RegisterDefaults();

// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// MSBuildLocator.RegisterDefaults();
//
// var workspace = MSBuildWorkspace.Create();
// workspace.WorkspaceFailed += (sender, args) =>
// {
//     ConsoleLogger.Default.WriteLineError("---------------------");
//     ConsoleLogger.Default.WriteLineError(args.Diagnostic.ToString());
//     ConsoleLogger.Default.WriteLineError("---------------------");
// };
// var project = await workspace.OpenProjectAsync(
//     "../../../../../samples/ASPNET_CleanArchitecture/AspNetSample.Api/AspNetSample.Api.csproj"
// );
//
// var compilation = await project.GetCompilationAsync();
// Debug.Assert(compilation != null);
// var generator = GeneratorExtensions.AsSourceGenerator(new IncrementalMediatorGenerator());
//
// GeneratorDriver driver = CSharpGeneratorDriver.Create(
//     new[] { generator },
//     parseOptions: (CSharpParseOptions)project.ParseOptions!
// );
//
// driver = driver.RunGeneratorsAndUpdateCompilation(compilation!, out var newCompilation, out var newDiagnostics);
//
// var result = driver.GetRunResult();
// Debug.Assert(result.Diagnostics.Length == 0);
