using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using BenchmarkDotNet.Running;
using Mediator.Benchmarks.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging;

[ConfigSource]
public class TelemetryOverheadBenchmarks
{
    private enum TelemetryMode
    {
        None,
        Metrics,
        Tracing,
        Both,
    }

    private enum ListenerMode
    {
        Off,
        On,
    }

    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute()
        {
            ServiceLifetime[] lifetimes =
            [
                ServiceLifetime.Singleton,
                ServiceLifetime.Scoped,
                ServiceLifetime.Transient,
            ];
            var telemetryModes = Enum.GetValues<TelemetryMode>();
            var listenerModes = Enum.GetValues<ListenerMode>();

            var jobs =
                from lifetime in lifetimes
                from telemetryMode in telemetryModes
                from listenerMode in listenerModes
                where listenerMode == ListenerMode.Off || telemetryMode != TelemetryMode.None
                select Job
                    .Default.WithArguments([
                        new MsBuildArgument(
                            $"/p:ExtraDefineConstants={GetExtraDefineConstants(lifetime, telemetryMode)}"
                        ),
                    ])
                    .WithEnvironmentVariable("ServiceLifetime", lifetime.ToString())
                    .WithEnvironmentVariable("IsLargeProject", "False")
                    .WithEnvironmentVariable("TelemetryMode", telemetryMode.ToString())
                    .WithEnvironmentVariable("ListenerMode", listenerMode.ToString())
                    .WithCustomBuildConfiguration($"{lifetime}/{telemetryMode}/{listenerMode}")
                    .WithId($"{lifetime}_{GetTelemetryOrder(telemetryMode)}_{telemetryMode}_{listenerMode}");

            Config = ManualConfig
                .CreateEmpty()
                .AddJob(jobs.ToArray())
                .AddColumn(new CustomColumn("ServiceLifetime", (_, c) => c.Job.Id.Split('_')[0]))
                .AddColumn(new CustomColumn("Telemetry", (_, c) => c.Job.Id.Split('_')[2]))
                .AddColumn(new CustomColumn("Listeners", (_, c) => c.Job.Id.Split('_')[3]))
                .AddColumn(CategoriesColumn.Default)
                .WithOption(ConfigOptions.KeepBenchmarkFiles, false)
                .HideColumns(Column.Arguments, Column.EnvironmentVariables, Column.BuildConfiguration, Column.Job)
                .WithOrderer(new TelemetryOrderer())
                .AddDiagnoser(MemoryDiagnoser.Default);
        }

        private static int GetTelemetryOrder(TelemetryMode telemetryMode) =>
            telemetryMode switch
            {
                TelemetryMode.None => 0,
                TelemetryMode.Metrics => 1,
                TelemetryMode.Tracing => 2,
                TelemetryMode.Both => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(telemetryMode), telemetryMode, null),
            };

        private static string GetExtraDefineConstants(ServiceLifetime lifetime, TelemetryMode telemetryMode)
        {
            var constants = $"Mediator_Lifetime_{lifetime}";
            return telemetryMode switch
            {
                TelemetryMode.None => constants,
                TelemetryMode.Metrics => $"{constants}%3BMediator_Telemetry_EnableMetrics",
                TelemetryMode.Tracing => $"{constants}%3BMediator_Telemetry_EnableTracing",
                TelemetryMode.Both =>
                    $"{constants}%3BMediator_Telemetry_EnableMetrics%3BMediator_Telemetry_EnableTracing",
                _ => throw new ArgumentOutOfRangeException(nameof(telemetryMode), telemetryMode, null),
            };
        }

        private sealed class TelemetryOrderer : IOrderer
        {
            public bool SeparateLogicalGroups => true;

            public IEnumerable<BenchmarkCase> GetExecutionOrder(
                ImmutableArray<BenchmarkCase> benchmarksCase,
                IEnumerable<BenchmarkLogicalGroupRule> order = null
            ) => GetOrdered(benchmarksCase);

            public IEnumerable<BenchmarkCase> GetSummaryOrder(
                ImmutableArray<BenchmarkCase> benchmarksCases,
                Summary summary
            ) => GetOrdered(benchmarksCases);

            public string GetHighlightGroupKey(BenchmarkCase benchmarkCase) => GetTelemetry(benchmarkCase);

            public string GetLogicalGroupKey(
                ImmutableArray<BenchmarkCase> allBenchmarkCases,
                BenchmarkCase benchmarkCase
            ) => GetTelemetry(benchmarkCase);

            public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(
                IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups,
                IEnumerable<BenchmarkLogicalGroupRule> order = null
            ) => logicalGroups.OrderBy(g => GetTelemetryOrder(g.Key));

            private static IEnumerable<BenchmarkCase> GetOrdered(ImmutableArray<BenchmarkCase> benchmarksCases)
            {
                return benchmarksCases
                    .Select(static benchmarkCase => (BenchmarkCase: benchmarkCase, Order: ParseOrder(benchmarkCase)))
                    .OrderBy(static x => x.Order.TelemetryOrder)
                    .ThenBy(static x => x.Order.MethodOrder)
                    .ThenBy(static x => x.Order.LifetimeOrder)
                    .ThenBy(static x => x.Order.ListenersOrder)
                    .Select(static x => x.BenchmarkCase);
            }

            private static string GetTelemetry(BenchmarkCase benchmarkCase)
            {
                var split = benchmarkCase.Job.Id.Split('_');
                return split[2];
            }

            private static int GetTelemetryOrder(string telemetry) =>
                telemetry switch
                {
                    nameof(TelemetryMode.None) => 0,
                    nameof(TelemetryMode.Metrics) => 1,
                    nameof(TelemetryMode.Tracing) => 2,
                    nameof(TelemetryMode.Both) => 3,
                    _ => int.MaxValue,
                };

            private static (int MethodOrder, int TelemetryOrder, int LifetimeOrder, int ListenersOrder) ParseOrder(
                BenchmarkCase benchmarkCase
            )
            {
                var split = benchmarkCase.Job.Id.Split('_');

                var methodOrder = benchmarkCase.Descriptor.WorkloadMethod.Name switch
                {
                    nameof(TelemetryOverheadBenchmarks.SendRequest_IMediator) => 0,
                    nameof(TelemetryOverheadBenchmarks.PublishNotification_IMediator) => 1,
                    nameof(TelemetryOverheadBenchmarks.CreateStream_IMediator) => 2,
                    _ => int.MaxValue,
                };

                var lifetimeOrder = split[0] switch
                {
                    nameof(ServiceLifetime.Singleton) => 0,
                    nameof(ServiceLifetime.Scoped) => 1,
                    nameof(ServiceLifetime.Transient) => 2,
                    _ => int.MaxValue,
                };

                var telemetryOrder = int.Parse(split[1], global::System.Globalization.CultureInfo.InvariantCulture);

                var listenersOrder = split[3] switch
                {
                    nameof(ListenerMode.Off) => 0,
                    nameof(ListenerMode.On) => 1,
                    _ => int.MaxValue,
                };

                return (methodOrder, telemetryOrder, lifetimeOrder, listenersOrder);
            }
        }
    }

    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Request _request;
    private SingleHandlerNotification _notification;
    private StreamRequest _streamRequest;
    private ActivityListener _activityListener;
    private MeterListener _meterListener;

    [GlobalSetup]
    public void Setup()
    {
        Fixture.Setup();

        var telemetryModeValue =
            Environment.GetEnvironmentVariable("TelemetryMode")
            ?? throw new InvalidOperationException("Missing TelemetryMode environment variable.");
        var listenerModeValue =
            Environment.GetEnvironmentVariable("ListenerMode")
            ?? throw new InvalidOperationException("Missing ListenerMode environment variable.");

        var telemetryMode = Enum.Parse<TelemetryMode>(telemetryModeValue);
        var listenerMode = Enum.Parse<ListenerMode>(listenerModeValue);

        var services = new ServiceCollection();
        services.AddMediator();

        _serviceProvider = services.BuildServiceProvider();
#pragma warning disable CS0162 // Unreachable code detected
        if (Mediator.ServiceLifetime == ServiceLifetime.Scoped)
        {
            _serviceScope = _serviceProvider.CreateScope();
            _serviceProvider = _serviceScope.ServiceProvider;
        }
#pragma warning restore CS0162 // Unreachable code detected

        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _request = new Request(Guid.NewGuid());
        _notification = new SingleHandlerNotification(Guid.NewGuid());
        _streamRequest = new StreamRequest(Guid.NewGuid());

        if (listenerMode == ListenerMode.On)
            EnableListeners(telemetryMode);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _meterListener?.Dispose();
        _activityListener?.Dispose();

        if (_serviceScope is not null)
            _serviceScope.Dispose();
        else
            (_serviceProvider as IDisposable)?.Dispose();
    }

    [Benchmark]
    [BenchmarkCategory("Request")]
    public ValueTask<Response> SendRequest_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification")]
    public ValueTask PublishNotification_IMediator()
    {
        return _mediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Stream")]
    public async ValueTask CreateStream_IMediator()
    {
        await foreach (var response in _mediator.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    private void EnableListeners(TelemetryMode telemetryMode)
    {
        if (telemetryMode is TelemetryMode.Tracing or TelemetryMode.Both)
        {
            _activityListener = new ActivityListener
            {
                ShouldListenTo = static _ => true,
                Sample = static (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllDataAndRecorded,
                SampleUsingParentId = static (ref ActivityCreationOptions<string> _) =>
                    ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = static _ => { },
                ActivityStopped = static _ => { },
            };
            ActivitySource.AddActivityListener(_activityListener);
        }

        if (telemetryMode is TelemetryMode.Metrics or TelemetryMode.Both)
        {
            _meterListener = new MeterListener
            {
                InstrumentPublished = static (instrument, listener) => listener.EnableMeasurementEvents(instrument),
            };
            _meterListener.SetMeasurementEventCallback<double>(static (_, _, _, _) => { });
            _meterListener.Start();
        }
    }
}
