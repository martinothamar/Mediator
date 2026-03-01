using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.SourceGenerator.Tests;

public sealed class ProjectTypeTests
{
    private static string Short(ServiceLifetime value) =>
        value switch
        {
            ServiceLifetime.Singleton => "Sg",
            ServiceLifetime.Scoped => "Sc",
            ServiceLifetime.Transient => "Tr",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

    private static string ShortCachingMode(string value) =>
        value switch
        {
            "Eager" => "E",
            "Lazy" => "L",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

    private static string ShortMessageType(string value) =>
        value switch
        {
            "Request" => "Req",
            "Query" => "Qry",
            "Command" => "Cmd",
            "Notification" => "Not",
            "StreamRequest" => "SReq",
            "StreamQuery" => "SQry",
            "StreamCommand" => "SCmd",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

    private static string GenerateMessagesAndHandlers(
        int n,
        ServiceLifetime? serviceLifetime = null,
        string? cachingMode = null,
        string manyOfMessageType = ""
    )
    {
        var code = new StringBuilder(
            $$"""
            using System;
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;
            using System.Runtime.CompilerServices;

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator(options =>
                    {
            """
        );

        if (serviceLifetime is not null)
        {
            code.AppendLine(
                $$"""
                        options.ServiceLifetime = ServiceLifetime.{{serviceLifetime}};
                """
            );
        }

        if (cachingMode is not null)
        {
            code.AppendLine(
                $$"""
                        options.CachingMode = CachingMode.{{cachingMode}};
                """
            );
        }

        code.AppendLine(
            """
                        });
                    }
                }

            """
        );

        string[] messageTypes = ["Request", "Query", "Command", "Notification"];
        foreach (var messageType in messageTypes)
        {
            var toGenerate = messageType == manyOfMessageType ? 17 : n;
            for (int i = 0; i < toGenerate; i++)
            {
                if (messageType == "Notification")
                {
                    code.AppendLine(
                        $"public sealed record Test{messageType}{i}() : I{messageType}; "
                            + $"public sealed record Test{messageType}{i}Handler : I{messageType}Handler<Test{messageType}{i}> {{ "
                            + $"public ValueTask Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => default; "
                            + $"}}"
                    );
                }
                else
                {
                    code.AppendLine(
                        $"public sealed record Test{messageType}{i}() : I{messageType}<int>; "
                            + $"public sealed record Test{messageType}{i}Handler : I{messageType}Handler<Test{messageType}{i}, int> {{ "
                            + $"public ValueTask<int> Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => new ValueTask<int>({i}); "
                            + $"}}"
                    );
                }
            }
            code.AppendLine();
        }

        foreach (var messageType in messageTypes)
        {
            if (messageType == "Notification")
                continue;

            var toGenerate = $"Stream{messageType}" == manyOfMessageType ? 17 : n;
            code.AppendLine();
            for (int i = 0; i < n; i++)
            {
                code.AppendLine(
                    $"public sealed record TestStream{messageType}{i}() : IStream{messageType}<int>; "
                        + $"public sealed record TestStream{messageType}{i}Handler : IStream{messageType}Handler<TestStream{messageType}{i}, int> {{ "
                        + $"public async IAsyncEnumerable<int> Handle(TestStream{messageType}{i} {messageType.ToLowerInvariant()}, [EnumeratorCancellation] CancellationToken cancellationToken) {{ for (int i = 0; i < 3; i++) {{ await Task.Yield(); yield return {i}; }} }} "
                        + $"}}"
                );
            }
        }

        return code.ToString();
    }

    [Theory, CombinatorialData]
    public async Task Project(
        [CombinatorialValues(16, 17)] int n,
        ServiceLifetime serviceLifetime,
        [CombinatorialValues("Eager", "Lazy")] string cachingMode
    )
    {
        var code = GenerateMessagesAndHandlers(n, serviceLifetime, cachingMode: cachingMode);

        var inputCompilation = Fixture.CreateLibrary(code);

        await inputCompilation
            .AssertAndVerify(
                Assertions.CompilesWithoutDiagnostics,
                result =>
                {
                    var model = result.Generator.CompilationModel;
                    Assert.NotNull(model);
                    Action<bool> assertMany = n > 16 ? Assert.True : Assert.False;
                    assertMany(model.HasManyRequests);
                    assertMany(model.HasManyQueries);
                    assertMany(model.HasManyCommands);
                    assertMany(model.HasManyStreamRequests);
                    assertMany(model.HasManyStreamQueries);
                    assertMany(model.HasManyStreamCommands);
                    assertMany(model.HasManyNotifications);

                    Assert.True(model.HasRequests);
                    Assert.True(model.HasQueries);
                    Assert.True(model.HasCommands);
                    Assert.True(model.HasStreamRequests);
                    Assert.True(model.HasStreamQueries);
                    Assert.True(model.HasStreamCommands);
                    Assert.True(model.HasNotifications);

                    if (cachingMode == "Eager")
                    {
                        Assert.True(model.CachingModeIsEager);
                        Assert.False(model.CachingModeIsLazy);
                    }
                    else
                    {
                        Assert.False(model.CachingModeIsEager);
                        Assert.True(model.CachingModeIsLazy);
                    }
                }
            )
            .UseTextForParameters($"n={n}_sl={Short(serviceLifetime)}_cm={ShortCachingMode(cachingMode)}");
    }

    [Theory, CombinatorialData]
    public async Task Project_Uneven(
        [CombinatorialValues(
            "Request",
            "Query",
            "Command",
            "Notification",
            "StreamRequest",
            "StreamQuery",
            "StreamCommand"
        )]
            string manyOfMessageType,
        ServiceLifetime serviceLifetime,
        [CombinatorialValues("Eager", "Lazy")] string cachingMode
    )
    {
        var code = GenerateMessagesAndHandlers(
            1,
            serviceLifetime,
            cachingMode: cachingMode,
            manyOfMessageType: manyOfMessageType
        );

        var inputCompilation = Fixture.CreateLibrary(code);

        await inputCompilation
            .AssertAndVerify(
                Assertions.CompilesWithoutDiagnostics,
                result =>
                {
                    var model = result.Generator.CompilationModel;
                    Assert.NotNull(model);

                    if (cachingMode == "Eager")
                    {
                        Assert.True(model.CachingModeIsEager);
                        Assert.False(model.CachingModeIsLazy);
                    }
                    else
                    {
                        Assert.False(model.CachingModeIsEager);
                        Assert.True(model.CachingModeIsLazy);
                    }
                }
            )
            .UseTextForParameters(
                $"mot={ShortMessageType(manyOfMessageType)}_sl={Short(serviceLifetime)}_cm={ShortCachingMode(cachingMode)}"
            );
    }

    [Theory, CombinatorialData]
    public async Task Project_Telemetry(
        ServiceLifetime serviceLifetime,
        [CombinatorialValues("Eager", "Lazy")] string cachingMode,
        bool enableMetrics,
        bool enableTracing
    )
    {
        var enableMetricsLiteral = enableMetrics.ToString().ToLowerInvariant();
        var enableTracingLiteral = enableTracing.ToString().ToLowerInvariant();
        var telemetryConfig = $$"""
                      options.Telemetry.EnableMetrics = {{enableMetricsLiteral}};
                      options.Telemetry.MeterName = "TestMeter";
                      options.Telemetry.EnableTracing = {{enableTracingLiteral}};
                      options.Telemetry.ActivitySourceName = "TestActivitySource";
            """;

        var inputCompilation = Fixture.CreateLibrary(
            $$"""
            using System;
            using System.Collections.Generic;
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
                        options.ServiceLifetime = ServiceLifetime.{{serviceLifetime}};
                        options.CachingMode = CachingMode.{{cachingMode}};
                        options.NotificationPublisherType = typeof(TaskWhenAllPublisher);
            {{telemetryConfig}}
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct StreamRequest(Guid Id) : IStreamRequest<Response>;
            public readonly record struct Response(Guid Id);
            public sealed record PingNotification(Guid Id) : INotification;

            public sealed class RequestHandler : IRequestHandler<Request, Response>, IStreamRequestHandler<StreamRequest, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));

                public async IAsyncEnumerable<Response> Handle(StreamRequest request, CancellationToken cancellationToken)
                {
                    await Task.Yield();
                    yield return new Response(request.Id);
                }
            }

            public sealed class PingNotificationHandler : INotificationHandler<PingNotification>
            {
                public ValueTask Handle(PingNotification notification, CancellationToken cancellationToken) => default;
            }
            """
        );

        await inputCompilation
            .AssertAndVerify(Assertions.CompilesWithoutErrorDiagnostics)
            .UseTextForParameters(
                $"sl={Short(serviceLifetime)}_cm={ShortCachingMode(cachingMode)}_em={(enableMetrics ? "1" : "0")}_et={(enableTracing ? "1" : "0")}"
            );
    }
}
