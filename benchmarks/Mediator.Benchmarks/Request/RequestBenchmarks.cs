using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable

namespace Mediator.Benchmarks.Request
{
    public readonly struct SomeRequest : IRequest<SomeResponse>
    {
        public readonly Guid Id;

        public SomeRequest(Guid id) => Id = id;
    }

    public readonly struct SomeResponse
    {
        public readonly Guid Id;

        public SomeResponse(Guid id) => Id = id;
    }

    public sealed class SomeClass : IRequestHandler<SomeRequest, SomeResponse>
    {
        public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken)
        {
            return new ValueTask<SomeResponse>(new SomeResponse(request.Id));
        }
    }

    [MemoryDiagnoser]
    public sealed class RequestBenchmarks
    {
        private IServiceProvider _serviceProvider;
        private IMediator _mediator;
        private SomeRequest _request;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddMediator();

            _serviceProvider = services.BuildServiceProvider();
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
            _request = new(Guid.NewGuid());
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }

        [Benchmark]
        public ValueTask<SomeResponse> SendRequest()
        {
            return _mediator.Send(_request);
        }
    }
}
