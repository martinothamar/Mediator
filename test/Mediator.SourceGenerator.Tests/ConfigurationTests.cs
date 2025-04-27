using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public async Task Test_ForEachAwaitPublisher_Default()
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
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                Assert.Equal("global::Mediator.ForeachAwaitPublisher", model.NotificationPublisherType.FullName);
            }
        );
    }

    [Fact]
    public async Task Test_TaskWhenAllPublisher_For_Notifications_AddMediator()
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

                    services.AddMediator(options =>
                    {
                        options.NotificationPublisherType = typeof(TaskWhenAllPublisher);
                    });
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                Assert.Equal("global::Mediator.TaskWhenAllPublisher", model.NotificationPublisherType.FullName);
            }
        );
    }

    [Fact]
    public async Task Test_TaskWhenAllPublisher_For_Notifications_Attribute()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            [assembly: MediatorOptions(NotificationPublisherType = typeof(TaskWhenAllPublisher))]

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator();
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                Assert.Equal("global::Mediator.TaskWhenAllPublisher", model.NotificationPublisherType.FullName);
            }
        );
    }

    [Fact]
    public async Task Test_PipelineBehaviors()
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

                    services.AddMediator(options =>
                    {
                        options.PipelineBehaviors =
                        [
                            typeof(GenericLoggerHandler<,>),
                            typeof(PingValidator),
                        ];
                    });
                }
            }

            public readonly record struct Ping(Guid Id) : IRequest<Pong>;

            public readonly record struct Pong(Guid Id);

            public sealed class GenericLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
                where TMessage : IMessage
            {
                public async ValueTask<TResponse> Handle(
                    TMessage message,
                    MessageHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                )
                {
                    try
                    {
                        var response = await next(message, cancellationToken);
                        return response;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                }
            }

            public sealed class PingValidator : IPipelineBehavior<Ping, Pong>
            {
                public ValueTask<Pong> Handle(
                    Ping request,
                    MessageHandlerDelegate<Ping, Pong> next,
                    CancellationToken cancellationToken
                )
                {
                    if (request.Id == default)
                        throw new ArgumentException("Invalid input");

                    return next(request, cancellationToken);
                }
            }

            public sealed class PingHandler : IRequestHandler<Ping, Pong>
            {
                public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
                {
                    return new ValueTask<Pong>(new Pong(request.Id));
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
            }
        );
    }
}
