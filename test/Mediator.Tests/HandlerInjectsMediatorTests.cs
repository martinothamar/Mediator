using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests;

public class HandlerInjectsMediatorTests
{
    public sealed record Request(Guid Id) : IRequest;

    public sealed class Handler : IRequestHandler<Request>
    {
        public static readonly ConcurrentBag<Guid> Ids = new();

        public Handler(IMediator mediator)
        {
            Assert.NotNull(mediator);
        }

        public ValueTask<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            Ids.Add(request.Id);
            return default;
        }
    }

    [Fact]
    public async Task Test_Invoke_Handler_Which_Injects_IMediator()
    {
        var (_, mediator) = Fixture.GetMediator();
        Assert.NotNull(mediator);

        var id = Guid.NewGuid();
        await mediator.Send(new Request(id), TestContext.Current.CancellationToken);
        Assert.Contains(id, Handler.Ids);
    }
}
