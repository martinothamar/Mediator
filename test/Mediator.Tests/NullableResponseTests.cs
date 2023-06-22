using Mediator.Tests.TestTypes;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();

        var response = await mediator.Send(new RequestWithNullableResponse(id));
        Assert.Null(response);

        var response2 = await concrete.Send(new RequestWithNullableResponse(id));
        Assert.Null(response2);
    }
}
