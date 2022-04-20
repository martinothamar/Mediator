using BenchmarkDotNet.Running;

ConsoleLogger.Default.WriteLine($"Running with lifetime: Impl={Mediator.Mediator.ServiceLifetime}");
ConsoleLogger.Default.WriteLine();

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
