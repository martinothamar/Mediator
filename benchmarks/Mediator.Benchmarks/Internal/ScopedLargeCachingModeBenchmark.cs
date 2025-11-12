using BenchmarkDotNet.Environments;
using Mediator.Benchmarks.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Internal;

[ConfigSource]
public class ScopedLargeCachingModeBenchmark
{
    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute()
        {
            ServiceLifetime[] lifetimes = [ServiceLifetime.Scoped];
            bool[] largeProjectOptions = [true];
            string[] cachingModes = ["Eager", "Lazy"];
            // TODO: currently broken because BenchmarkDotNet doens't have .NET 10 yet
            bool[] aot = [false, true];

            var jobs =
                from lifetime in lifetimes
                from largeProject in largeProjectOptions
                from cachingMode in cachingModes
                from aotMode in aot
                select Job
                    .Default.WithRuntime(aotMode ? NativeAotRuntime.Net80 : CoreRuntime.Core80)
                    .WithArguments([
                        new MsBuildArgument(
                            $"/p:ExtraDefineConstants=Mediator_Lifetime_{lifetime}"
                                + $"%3BMediator_CachingMode_{cachingMode}"
                                + (largeProject ? "%3BMediator_Large_Project" : "")
                        ),
                    ])
                    .WithEnvironmentVariable("ServiceLifetime", lifetime.ToString())
                    .WithEnvironmentVariable("IsLargeProject", $"{largeProject}")
                    .WithEnvironmentVariable("CachingMode", cachingMode)
                    .WithCustomBuildConfiguration($"{lifetime}/{cachingMode}/{largeProject}/{aotMode}")
                    .WithId($"{lifetime}_{cachingMode}_{largeProject}_{aotMode}");

            Config = ManualConfig
                .CreateEmpty()
                .AddJob(jobs.ToArray())
                .AddColumn(new CustomColumn("CachingMode", (_, c) => c.Job.Id.Split('_')[1]))
                .AddColumn(
                    new CustomColumn("Project type", (_, c) => c.Job.Id.Split('_')[2] == "True" ? "Large" : "Small")
                )
                .AddColumn(new CustomColumn("Compiler", (_, c) => c.Job.Id.Split('_')[3] == "True" ? "AOT" : "JIT"))
                .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByJob)
                .WithOption(ConfigOptions.KeepBenchmarkFiles, false)
                // .WithOption(ConfigOptions.DisableParallelBuild, true)
                .HideColumns(Column.Arguments, Column.EnvironmentVariables, Column.BuildConfiguration, Column.Job)
                .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
                .AddColumn(RankColumn.Arabic)
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared))
                .AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    private Request _req;

    [GlobalSetup]
    public void Setup()
    {
        _req = new Request(default);
    }

    [Benchmark]
    public ValueTask<ServiceCollection> AddMediator()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        return new ValueTask<ServiceCollection>(services);
    }

    [Benchmark]
    public async ValueTask<object> ResolveMediator()
    {
        var services = new ServiceCollection();
        services.AddMediator();

        await using var provider = services.BuildServiceProvider();

        await using var scope = provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return mediator;
    }

    // Will need to expose `GetRequestHandler` as internal to run these
    //     [Benchmark]
    //     public async ValueTask<object> GetRequestHandler()
    //     {
    // #if Mediator_Large_Project
    //         var services = new ServiceCollection();
    //         services.AddMediator();

    //         await using var provider = services.BuildServiceProvider();

    //         await using var scope = provider.CreateAsyncScope();
    //         var mediator = scope.ServiceProvider.GetRequiredService<global::Mediator.Mediator>();
    //         return mediator.GetRequestHandler(_req);
    // #else
    //         await Task.Yield();
    //         return null!;
    // #endif
    //     }

    [Benchmark]
    public async ValueTask<object> Send()
    {
        var services = new ServiceCollection();
        services.AddMediator();

        await using var provider = services.BuildServiceProvider();

        await using var scope = provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var message = new Request(default);
        return await mediator.Send(message, default);
    }
}
