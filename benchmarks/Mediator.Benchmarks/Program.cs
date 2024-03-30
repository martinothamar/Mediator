using BenchmarkDotNet.Running;
using Mediator.Benchmarks.Notification;
using Microsoft.Build.Locator;

ConsoleLogger.Default.WriteLine($"Running with lifetime: Impl={Mediator.Mediator.ServiceLifetime}");
ConsoleLogger.Default.WriteLine();

MSBuildLocator.RegisterDefaults();

// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// var bench = new NotificationBenchmarks();
// bench.Setup();
// for (int i = 0; i < 1_000_000_000; i++)
// {
//     await bench.SendNotification_IMediator();
// }
