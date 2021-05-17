using BenchmarkDotNet.Attributes;
using MediatR;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable

namespace Mediator.Benchmarks.Request
{
    public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>, MediatR.IRequest<SomeResponse>;

    public sealed record SomeResponse(Guid Id);

    public sealed record Wat : MediatR.IRequest;

    public sealed class SomeClass : IRequestHandler<SomeRequest, SomeResponse>, MediatR.IRequestHandler<SomeRequest, SomeResponse>, IAsyncRequestHandler<SomeRequest, SomeResponse>
    {
        private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());

        private static readonly Task<SomeResponse> _tResponse = Task.FromResult(_response);
        private static ValueTask<SomeResponse> _vtResponse => new ValueTask<SomeResponse>(_response);

        ValueTask<SomeResponse> IRequestHandler<SomeRequest, SomeResponse>.Handle(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;

        Task<SomeResponse> MediatR.IRequestHandler<SomeRequest, SomeResponse>.Handle(SomeRequest request, CancellationToken cancellationToken) => _tResponse;

        ValueTask<SomeResponse> IAsyncRequestHandlerCore<SomeRequest, SomeResponse>.InvokeAsync(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;
    }

    [MemoryDiagnoser]
    public class RequestBenchmarks
    {
        private IServiceProvider _serviceProvider;
        private IMediator _mediator;
        private MediatR.IMediator _mediatr;
        private IAsyncRequestHandler<SomeRequest, SomeResponse> _messagePipeHandler;
        private IRequestHandler<SomeRequest, SomeResponse> _handler;
        private SomeRequest _request;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddMediator();
            services.AddMediatR(typeof(SomeClass).Assembly);
            services.AddMessagePipe();

            _serviceProvider = services.BuildServiceProvider();
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();
            _messagePipeHandler = _serviceProvider.GetRequiredService<IAsyncRequestHandler<SomeRequest, SomeResponse>>();
            _handler = _serviceProvider.GetRequiredService<SomeClass>();
            _request = new(Guid.NewGuid());
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }

        [Benchmark(Baseline = true)]
        public Task<SomeResponse> SendRequest_MediatR()
        {
            _mediatr.Send(new Wat());
            return _mediatr.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendRequest_Mediator()
        {
            return _mediator.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendRequest_MessagePipe()
        {
            return _messagePipeHandler.InvokeAsync(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendRequest_Baseline()
        {
            return _handler.Handle(_request, CancellationToken.None);
        }
    }
}
