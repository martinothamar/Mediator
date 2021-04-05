using BenchmarkDotNet.Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable

namespace Mediator.Benchmarks.Request
{
    public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>, MediatR.IRequest<SomeResponse>;

    public sealed record SomeResponse(Guid Id);

    public sealed class SomeClass : IRequestHandler<SomeRequest, SomeResponse>, MediatR.IRequestHandler<SomeRequest, SomeResponse>
    {
        private static readonly Task<SomeResponse> _response = Task.FromResult(new SomeResponse(Guid.NewGuid()));

        ValueTask<SomeResponse> IRequestHandler<SomeRequest, SomeResponse>.Handle(SomeRequest request, CancellationToken cancellationToken)
        {
            return new ValueTask<SomeResponse>(_response.Result);
        }

        Task<SomeResponse> MediatR.IRequestHandler<SomeRequest, SomeResponse>.Handle(SomeRequest request, CancellationToken cancellationToken)
        {
            return _response;
        }
    }

    [MemoryDiagnoser]
    public class RequestBenchmarks
    {
        private IServiceProvider _serviceProvider;
        private IMediator _mediator;
        private MediatR.IMediator _mediatr;
        private IRequestHandler<SomeRequest, SomeResponse> _handler;
        private SomeRequest _request;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddMediator();
            services.AddMediatR(typeof(SomeClass).Assembly);

            _serviceProvider = services.BuildServiceProvider();
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();
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
            return _mediatr.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendRequest_Mediator()
        {
            return _mediator.Send(_request, CancellationToken.None);
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendRequest_Baseline()
        {
            return _handler.Handle(_request, CancellationToken.None);
        }
    }
}
