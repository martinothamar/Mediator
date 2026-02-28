using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Telemetry.Tests;

[Collection("Non-Parallel")]
public sealed class TelemetryRuntimeTests
{
    [Fact]
    public async Task Test_Telemetry_Emits_Metrics_For_Request_Notification_And_Stream()
    {
        TelemetryNotificationHandler.Reset();
        using var capture = new MeterCapture(TestConfiguration.MeterName);
        using var ctx = CreateServiceProvider();
        var mediator = ctx.Services.GetRequiredService<IMediator>();
        var ct = TestContext.Current.CancellationToken;
        var id = Guid.NewGuid();

        var response = await mediator.Send(new TelemetryRequest(id), ct);
        response.Id.Should().Be(id);

        await mediator.Publish(new TelemetryNotification(id), ct);

        var streamValues = new List<TelemetryResponse>();
        await foreach (var value in mediator.CreateStream(new TelemetryStreamRequest(id), ct))
            streamValues.Add(value);

        streamValues.Should().HaveCount(3);
        TelemetryNotificationHandler.CallCount.Should().Be(1);

        if (TestConfiguration.EnableMetrics)
        {
            var destinations = capture.Measurements.Select(x => x.DestinationName).ToArray();
            destinations.Should().Contain(nameof(TelemetryRequest));
            destinations.Should().Contain(nameof(TelemetryNotification));
            destinations.Should().Contain(nameof(TelemetryStreamRequest));
            capture.Measurements.Should().OnlyContain(x => x.InstrumentName == "messaging.process.duration");
            capture.Measurements.Should().OnlyContain(x => x.OperationName == "process");
        }
        else
        {
            capture.Measurements.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Test_Telemetry_NotificationPublisher_Resolution_Follows_Configuration()
    {
        TelemetryNotificationHandler.Reset();
        using var capture = new MeterCapture(TestConfiguration.MeterName);
        using var ctx = CreateServiceProvider();
        var mediator = ctx.Services.GetRequiredService<IMediator>();
        var ct = TestContext.Current.CancellationToken;
        var id = Guid.NewGuid();

        await mediator.Publish(new TelemetryNotification(id), ct);

        TelemetryNotificationHandler.CallCount.Should().Be(1);

        var publisher = ctx.Services.GetRequiredService<INotificationPublisher>();
        publisher.Should().BeOfType(TestConfiguration.NotificationPublisherType);

        if (TestConfiguration.EnableMetrics)
            capture.Measurements.Should().Contain(x => x.DestinationName == nameof(TelemetryNotification));
        else
            capture.Measurements.Should().BeEmpty();
    }

    [Fact]
    public async Task Test_Telemetry_Emits_ErrorType_For_Failing_Request_Notification_And_Stream()
    {
        using var capture = new MeterCapture(TestConfiguration.MeterName);
        using var ctx = CreateServiceProvider();
        var mediator = ctx.Services.GetRequiredService<IMediator>();
        var ct = TestContext.Current.CancellationToken;
        var id = Guid.NewGuid();

        await FluentActions
            .Awaiting(() => mediator.Send(new FailingTelemetryRequest(id), ct).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>();

        await FluentActions
            .Awaiting(() => mediator.Publish(new FailingTelemetryNotification(id), ct).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>();

        await FluentActions
            .Awaiting(async () =>
            {
                await foreach (var _ in mediator.CreateStream(new FailingTelemetryStreamRequest(id), ct)) { }
            })
            .Should()
            .ThrowAsync<InvalidOperationException>();

        if (TestConfiguration.EnableMetrics)
        {
            var errorType = typeof(InvalidOperationException).FullName;
            capture
                .Measurements.Should()
                .Contain(x => x.DestinationName == nameof(FailingTelemetryRequest) && x.ErrorType == errorType);
            capture
                .Measurements.Should()
                .Contain(x => x.DestinationName == nameof(FailingTelemetryNotification) && x.ErrorType == errorType);
            capture
                .Measurements.Should()
                .Contain(x => x.DestinationName == nameof(FailingTelemetryStreamRequest) && x.ErrorType == errorType);
        }
        else
        {
            capture.Measurements.Should().BeEmpty();
        }
    }

    private static TestServiceProviderContext CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddMediator();

        return new TestServiceProviderContext(
            services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })
        );
    }

    public sealed record TelemetryRequest(Guid Id) : IRequest<TelemetryResponse>;

    public sealed record TelemetryStreamRequest(Guid Id) : IStreamRequest<TelemetryResponse>;

    public sealed record TelemetryNotification(Guid Id) : INotification;

    public sealed record FailingTelemetryRequest(Guid Id) : IRequest<TelemetryResponse>;

    public sealed record FailingTelemetryStreamRequest(Guid Id) : IStreamRequest<TelemetryResponse>;

    public sealed record FailingTelemetryNotification(Guid Id) : INotification;

    public sealed record TelemetryResponse(Guid Id, int Index);

    public sealed class TelemetryHandler
        : IRequestHandler<TelemetryRequest, TelemetryResponse>,
            IStreamRequestHandler<TelemetryStreamRequest, TelemetryResponse>
    {
        public ValueTask<TelemetryResponse> Handle(TelemetryRequest request, CancellationToken cancellationToken)
        {
            return new(new TelemetryResponse(request.Id, 0));
        }

        public async IAsyncEnumerable<TelemetryResponse> Handle(
            TelemetryStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            for (var i = 1; i <= 3; i++)
            {
                await Task.Yield();
                yield return new TelemetryResponse(request.Id, i);
            }
        }
    }

    public sealed class TelemetryNotificationHandler : INotificationHandler<TelemetryNotification>
    {
        private static int _callCount;

        public static int CallCount => Volatile.Read(ref _callCount);

        public static void Reset() => Volatile.Write(ref _callCount, 0);

        public ValueTask Handle(TelemetryNotification notification, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _callCount);
            return default;
        }
    }

    public sealed class FailingTelemetryHandler
        : IRequestHandler<FailingTelemetryRequest, TelemetryResponse>,
            IStreamRequestHandler<FailingTelemetryStreamRequest, TelemetryResponse>
    {
        public ValueTask<TelemetryResponse> Handle(FailingTelemetryRequest request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("request failed");
        }

        public async IAsyncEnumerable<TelemetryResponse> Handle(
            FailingTelemetryStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            await Task.Yield();
            yield return new TelemetryResponse(request.Id, 0);
            throw new InvalidOperationException("stream failed");
        }
    }

    public sealed class FailingTelemetryNotificationHandler : INotificationHandler<FailingTelemetryNotification>
    {
        public ValueTask Handle(FailingTelemetryNotification notification, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("notification failed");
        }
    }

    private readonly record struct MeasurementData(
        string InstrumentName,
        double Value,
        string? DestinationName,
        string? OperationName,
        string? ErrorType
    );

    private sealed class MeterCapture : IDisposable
    {
        private readonly ConcurrentQueue<MeasurementData> _measurements = new();
        private readonly MeterListener _listener;

        public IReadOnlyCollection<MeasurementData> Measurements => _measurements;

        public MeterCapture(string meterName)
        {
            _listener = new MeterListener();
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == meterName)
                    listener.EnableMeasurementEvents(instrument);
            };
            _listener.SetMeasurementEventCallback<double>(
                (instrument, measurement, tags, state) =>
                {
                    string? destinationName = null;
                    string? operationName = null;
                    string? errorType = null;

                    foreach (var tag in tags)
                    {
                        if (tag.Key == "messaging.destination.name")
                            destinationName = tag.Value?.ToString();
                        else if (tag.Key == "messaging.operation.name")
                            operationName = tag.Value?.ToString();
                        else if (tag.Key == "error.type")
                            errorType = tag.Value?.ToString();
                    }

                    _measurements.Enqueue(new(instrument.Name, measurement, destinationName, operationName, errorType));
                }
            );
            _listener.Start();
        }

        public void Dispose() => _listener.Dispose();
    }

    private sealed class TestServiceProviderContext : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope? _scope;

        public IServiceProvider Services { get; }

        public TestServiceProviderContext(ServiceProvider root)
        {
            _root = root;
            if (TestConfiguration.CreateServiceScope)
            {
                _scope = _root.CreateScope();
                Services = _scope.ServiceProvider;
            }
            else
            {
                Services = _root;
            }
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _root.Dispose();
        }
    }
}
