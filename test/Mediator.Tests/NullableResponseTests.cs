using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;

namespace Mediator.Tests;

public sealed record RequestWithNullableResponse(Guid Id) : IRequest<SomeResponse?>;

public sealed class RequestWithNullableResponseHandler : IRequestHandler<RequestWithNullableResponse, SomeResponse?>
{
    public ValueTask<SomeResponse?> Handle(RequestWithNullableResponse request, CancellationToken cancellationToken) =>
        new ValueTask<SomeResponse?>(default(SomeResponse?));
}

public class NullableResponseTests
{
    [Fact]
    public async Task Test_Request_With_Nullable_Response()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new RequestWithNullableResponse(id), ct);
        Assert.Null(response);

        var response2 = await concrete.Send(new RequestWithNullableResponse(id), ct);
        Assert.Null(response2);
    }
}
