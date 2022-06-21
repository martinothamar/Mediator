using BenchmarkDotNet.Running;
using Microsoft.Build.Locator;

ConsoleLogger.Default.WriteLine($"Running with lifetime: Impl={Mediator.Mediator.ServiceLifetime}");
ConsoleLogger.Default.WriteLine();

MSBuildLocator.RegisterDefaults();

// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
