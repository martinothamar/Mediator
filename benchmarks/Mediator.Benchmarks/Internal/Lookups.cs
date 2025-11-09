using System.Collections.Frozen;
using Mediator.Benchmarks.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Internal;

[ConfigSource]
public class LookupsBenchmarks
{
    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute()
        {
            var job = Job
                .Default.WithArguments([new MsBuildArgument($"/p:ExtraDefineConstants=Mediator_Large_Project")])
                .WithEnvironmentVariable("IsLargeProject", $"{true}")
                .WithId($"LargeProject");

            Config = ManualConfig
                .CreateEmpty()
                .AddJob(job)
                .WithOption(ConfigOptions.KeepBenchmarkFiles, false)
                .HideColumns(Column.Arguments, Column.EnvironmentVariables, Column.BuildConfiguration, Column.Job)
                .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
                .AddColumn(RankColumn.Arabic)
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared))
                .AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    private IServiceProvider _sp;
    private Dictionary<Type, object> _dict;
    private FrozenDictionary<Type, object> _frozenDict;
    private Type _serviceType;

    private sealed record DummyHandler();

    private sealed record LookupHandler(Dictionary<Type, object> Handlers);

    [GlobalSetup]
    public void Setup()
    {
        var types = typeof(LookupsBenchmarks)
            .Assembly.DefinedTypes.Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IBaseRequest)))
            .ToArray();
        var handlers = types.Select(t => (ServiceType: (Type)t, Instance: new DummyHandler())).ToArray();

        var services = new ServiceCollection();
        var dict = new Dictionary<Type, object>();

        foreach (var (serviceType, instance) in handlers)
        {
            services.AddSingleton(serviceType, _ => instance);
            // services.AddScoped(serviceType, _ => instance);
            dict.Add(serviceType, instance);
        }
        services.AddSingleton(_ => new LookupHandler(dict));
        _sp = services.BuildServiceProvider();
        _dict = dict;
        _frozenDict = dict.ToFrozenDictionary();
        _serviceType = typeof(Request);
    }

    // [Benchmark]
    // public object BuildFrozen() => _dict.ToFrozenDictionary();

    // [Benchmark]
    // public object DI() => _sp.GetRequiredService(_serviceType);

    [Benchmark]
    public object DIScoped()
    {
        using var scope = _sp.CreateScope();
        return scope.ServiceProvider.GetRequiredService(_serviceType);
    }

    [Benchmark]
    public object DIScopedThroughLookup()
    {
        using var scope = _sp.CreateScope();
        return scope.ServiceProvider.GetRequiredService<LookupHandler>().Handlers[_serviceType];
    }

    // [Benchmark]
    // public object Dict() => _dict[_serviceType];

    // [Benchmark]
    // public object FrozenDict() => _frozenDict[_serviceType];
}
