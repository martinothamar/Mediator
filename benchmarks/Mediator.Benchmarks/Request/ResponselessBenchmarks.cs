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
    public sealed record SomeResponselessRequest(Guid Id) : IRequest, MediatR.IRequest;

    public sealed class SomeResponselessHandlerClass : IRequestHandler<SomeResponselessRequest>, MediatR.IRequestHandler<SomeResponselessRequest>, IAsyncRequestHandler<SomeResponselessRequest, Unit>
    {
        public ValueTask<Unit> Handle(SomeResponselessRequest request, CancellationToken cancellationToken) => default;

        public ValueTask<Unit> InvokeAsync(SomeResponselessRequest request, CancellationToken cancellationToken = default) => default;

        Task<MediatR.Unit> MediatR.IRequestHandler<SomeResponselessRequest, MediatR.Unit>.Handle(SomeResponselessRequest request, CancellationToken cancellationToken) => MediatR.Unit.Task;
    }

    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public class ResponselessBenchmarks
    {
        private IServiceProvider _serviceProvider;
        private IMediator _mediator;
        private Mediator _concreteMediator;
        private MediatR.IMediator _mediatr;
        private IAsyncRequestHandler<SomeResponselessRequest, Unit> _messagePipeHandler;
        private SomeResponselessHandlerClass _handler;
        private SomeResponselessRequest _request;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddMediator();
            services.AddMediatR(typeof(SomeResponselessHandlerClass).Assembly);
            services.AddMessagePipe();

            _serviceProvider = services.BuildServiceProvider();
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            _concreteMediator = _serviceProvider.GetRequiredService<Mediator>();
            _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();
            _messagePipeHandler = _serviceProvider.GetRequiredService<IAsyncRequestHandler<SomeResponselessRequest, Unit>>();
            _handler = _serviceProvider.GetRequiredService<SomeResponselessHandlerClass>();
            _request = new(Guid.NewGuid());
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }

        [Benchmark(Baseline = true)]
        public Task<MediatR.Unit> SendResponselessRequest_MediatR()
        {
            return _mediatr.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<Unit> SendResponselessRequest_Mediator()
        {
            return _mediator.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<Unit> SendResponselessRequest_Mediator_Concrete()
        {
            return _concreteMediator.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<Unit> SendResponselessRequest_MessagePipe()
        {
            return _messagePipeHandler.InvokeAsync(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<Unit> SendResponselessRequest_Baseline()
        {
            return _handler.Handle(_request, CancellationToken.None);
        }
    }
}
