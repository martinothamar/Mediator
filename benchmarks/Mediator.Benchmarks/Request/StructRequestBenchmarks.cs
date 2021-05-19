using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MediatR;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable

namespace Mediator.Benchmarks.Request
{
    public readonly struct SomeStructRequest : IRequest<SomeResponse>, MediatR.IRequest<SomeResponse>
    {
        public readonly Guid Id;
        public readonly Guid CorrelationId;
        public readonly Guid CausationId;
        public readonly DateTimeOffset TimeStamp;
        public readonly uint Version;

        public SomeStructRequest(Guid id)
        {
            Id = id;
            CorrelationId = Guid.NewGuid();
            CausationId = Guid.NewGuid();
            TimeStamp = DateTimeOffset.UtcNow;
            Version = 1;
        }
    }

    public sealed class SomeStructHandler : IRequestHandler<SomeStructRequest, SomeResponse>, MediatR.IRequestHandler<SomeStructRequest, SomeResponse>, IAsyncRequestHandler<SomeStructRequest, SomeResponse>
    {
        private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());

        private static readonly Task<SomeResponse> _tResponse = Task.FromResult(_response);
        private static ValueTask<SomeResponse> _vtResponse => new ValueTask<SomeResponse>(_response);

        public ValueTask<SomeResponse> Handle(SomeStructRequest request, CancellationToken cancellationToken) => _vtResponse;

        Task<SomeResponse> MediatR.IRequestHandler<SomeStructRequest, SomeResponse>.Handle(SomeStructRequest request, CancellationToken cancellationToken) => _tResponse;

        public ValueTask<SomeResponse> InvokeAsync(SomeStructRequest request, CancellationToken cancellationToken) => _vtResponse;
    }

    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public class StructRequestBenchmarks
    {
        private IServiceProvider _serviceProvider;
        private IMediator _mediator;
        private Mediator _concreteMediator;
        private MediatR.IMediator _mediatr;
        private IAsyncRequestHandler<SomeStructRequest, SomeResponse> _messagePipeHandler;
        private SomeStructHandler _handler;
        private SomeStructRequest _request;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddMediator();
            services.AddMediatR(typeof(SomeStructHandler).Assembly);
            services.AddMessagePipe();

            _serviceProvider = services.BuildServiceProvider();
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            _concreteMediator = _serviceProvider.GetRequiredService<Mediator>();
            _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();
            _messagePipeHandler = _serviceProvider.GetRequiredService<IAsyncRequestHandler<SomeStructRequest, SomeResponse>>();
            _handler = _serviceProvider.GetRequiredService<SomeStructHandler>();
            _request = new(Guid.NewGuid());
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }

        [Benchmark(Baseline = true)]
        public Task<SomeResponse> SendStructRequest_MediatR()
        {
            return _mediatr.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendStructRequest_Mediator()
        {
            return _mediator.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendStructRequest_Mediator_Concrete()
        {
            return _concreteMediator.Send(in _request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendStructRequest_MessagePipe()
        {
            return _messagePipeHandler.InvokeAsync(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendStructRequest_Baseline()
        {
            return _handler.Handle(_request, CancellationToken.None);
        }
    }
}
