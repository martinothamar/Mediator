using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

public sealed class BasicTests
{
    [Fact]
    public async Task Multiple_Request_Handlers_One_Class()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator();
                }
            }

            public readonly record struct Request0(Guid Id) : IRequest<Response>;
            public readonly record struct Request1(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);
            public sealed class RequestHandler : IRequestHandler<Request0, Response>, IRequestHandler<Request1, Response>
            {
                public ValueTask<Response> Handle(Request0 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
                public ValueTask<Response> Handle(Request1 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Multiple_Notification_Handlers_One_Class()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator();
                }
            }

            public readonly record struct Notification0() : INotification;
            public readonly record struct Notification1() : INotification;
            public sealed class RequestHandler : INotificationHandler<Notification0>, INotificationHandler<Notification1>
            {
                public ValueTask Handle(Notification0 request, CancellationToken cancellationToken) =>
                    default;
                public ValueTask Handle(Notification1 request, CancellationToken cancellationToken) =>
                    default;
            }
            """
        );

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }
}
