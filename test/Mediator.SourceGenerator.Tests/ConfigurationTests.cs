using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.SourceGenerator.Tests;

public sealed class ConfigurationTests
{
    [Theory]
    [CombinatorialData]
    public async Task Test_GenerateTypesAsInternal(bool value)
    {
        var inputCompilation = Fixture.CreateLibrary(
            $$"""
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
                        options.GenerateTypesAsInternal = {{value.ToString().ToLowerInvariant()}};
                    });
                }
            }
            """
        );

        await inputCompilation
            .AssertAndVerify(ignoreGeneratedResult: null, Assertions.CompilesWithoutDiagnostics)
            .UseParameters(value);
    }

    [Fact]
    public async Task Test_GenerateTypesAsInternal_Non_Literal()
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
                private static readonly bool _generateTypesAsInternal = true;

                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator(options =>
                    {
                        options.GenerateTypesAsInternal = _generateTypesAsInternal;
                    });
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify();
    }

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
    public async Task Test_Assemblies_TypeOf()
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
                        options.Assemblies =
                        [
                            typeof(Program),
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Assemblies_TypeOf_Assembly()
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
                        options.Assemblies =
                        [
                            typeof(Program).Assembly,
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Assemblies_Duplicate_Reference()
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
                        options.Assemblies =
                        [
                            typeof(Program),
                            typeof(Program),
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify();
    }

    [Fact]
    public async Task Test_Assemblies_Duplicate_Configuration()
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
                        options.Assemblies =
                        [
                            typeof(Program),
                        ];
                        options.Assemblies =
                        [
                            typeof(Program),
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify();
    }

    [Fact]
    public async Task Test_Assemblies_Mediator_Abstractions_Reference()
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
                        options.Assemblies =
                        [
                            typeof(Unit),
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify();
    }

    [Fact]
    public async Task Test_Assemblies_Invalid_Reference()
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
                        options.Assemblies =
                        [
                            typeof(object),
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify();
    }

    [Fact]
    public async Task Test_Assemblies_Valid_Reference()
    {
        var referencedLibrary1 = Fixture
            .CreateLibrary(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Mediator;

                namespace TestCode.Library1;

                public readonly record struct Request1(Guid Id) : IRequest<Response1>;
                public readonly record struct Response1(Guid Id);
                public sealed class Request1Handler : IRequestHandler<Request1, Response1>
                {
                    public ValueTask<Response1> Handle(Request1 request, CancellationToken cancellationToken) =>
                        new ValueTask<Response1>(new Response1(request.Id));
                }
                """
            )
            .WithAssemblyName("TestCode.Library1");

        var referencedLibrary2 = Fixture
            .CreateLibrary(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Mediator;

                namespace TestCode.Library2;

                public readonly record struct Request2(Guid Id) : IRequest<Response2>;
                public readonly record struct Response2(Guid Id);
                public sealed class Request2Handler : IRequestHandler<Request2, Response2>
                {
                    public ValueTask<Response2> Handle(Request2 request, CancellationToken cancellationToken) =>
                        new ValueTask<Response2>(new Response2(request.Id));
                }
                """
            )
            .WithAssemblyName("TestCode.Library2");

        var mainLibrary0 = Fixture
            .CreateLibrary(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Mediator;
                using Microsoft.Extensions.DependencyInjection;
                using TestCode.Library1;

                namespace TestCode;

                public class Program
                {
                    public static void Main()
                    {
                        var services = new ServiceCollection();

                        services.AddMediator(options =>
                        {
                            options.Assemblies =
                            [
                                typeof(Request0),
                                typeof(Request1),
                            ];
                        });
                    }
                }

                public readonly record struct Request0(Guid Id) : IRequest<Response0>;
                public readonly record struct Response0(Guid Id);
                public sealed class Request0Handler : IRequestHandler<Request0, Response0>
                {
                    public ValueTask<Response0> Handle(Request0 request, CancellationToken cancellationToken) =>
                        new ValueTask<Response0>(new Response0(request.Id));
                }
                """
            )
            .WithAssemblyName("TestCode");
        mainLibrary0 = mainLibrary0.AddReferences(
            referencedLibrary1.ToMetadataReference(),
            referencedLibrary2.ToMetadataReference()
        );

        await mainLibrary0.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_PipelineBehaviors()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using System.Collections.Generic;
            using System.Runtime.CompilerServices;
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
                            typeof(GenericBehavior<,>),
                            typeof(GenericRequestBehavior<,>),
                            typeof(GenericQueryBehavior<,>),
                            typeof(ConcreteBehavior),
                            typeof(GenericExceptionHandler<,>),
                            typeof(GenericPreProcessor<,>),
                        ];
                        options.StreamPipelineBehaviors =
                        [
                            typeof(StreamGenericBehavior<,>),
                            typeof(StreamGenericRequestBehavior<,>),
                            typeof(StreamGenericQueryBehavior<,>),
                            typeof(StreamConcreteBehavior),
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public readonly record struct StreamRequest(Guid Id) : IStreamRequest<Response>;

            public sealed class GenericBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
                where TMessage : IMessage
            {
                public ValueTask<TResponse> Handle(
                    TMessage message,
                    MessageHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }

            public sealed class GenericPreProcessor<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
                where TMessage : IMessage
            {
                protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken) =>
                    default;
            }

            public sealed class GenericExceptionHandler<TMessage, TResponse> : MessageExceptionHandler<TMessage, TResponse>
                where TMessage : IMessage
            {
                protected override ValueTask<ExceptionHandlingResult<TResponse>> Handle(
                    TMessage message,
                    Exception exception,
                    CancellationToken cancellationToken
                )
                {
                    return NotHandled;
                }
            }

            public sealed class StreamGenericBehavior<TMessage, TResponse> : IStreamPipelineBehavior<TMessage, TResponse>
                where TMessage : IStreamMessage
            {
                public IAsyncEnumerable<TResponse> Handle(
                    TMessage message,
                    StreamHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }

            public sealed class GenericRequestBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
                where TMessage : IRequest<TResponse>
            {
                public ValueTask<TResponse> Handle(
                    TMessage message,
                    MessageHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }

            public sealed class StreamGenericRequestBehavior<TMessage, TResponse> : IStreamPipelineBehavior<TMessage, TResponse>
                where TMessage : IStreamRequest<TResponse>
            {
                public IAsyncEnumerable<TResponse> Handle(
                    TMessage message,
                    StreamHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }

            public sealed class GenericQueryBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
                where TMessage : IQuery<TResponse>
            {
                public ValueTask<TResponse> Handle(
                    TMessage message,
                    MessageHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }

            public sealed class StreamGenericQueryBehavior<TMessage, TResponse> : IStreamPipelineBehavior<TMessage, TResponse>
                where TMessage : IStreamQuery<TResponse>
            {
                public IAsyncEnumerable<TResponse> Handle(
                    TMessage message,
                    StreamHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }

            public sealed class ConcreteBehavior : IPipelineBehavior<Request, Response>
            {
                public ValueTask<Response> Handle(
                    Request request,
                    MessageHandlerDelegate<Request, Response> next,
                    CancellationToken cancellationToken
                ) => next(request, cancellationToken);
            }

            public sealed class StreamConcreteBehavior : IStreamPipelineBehavior<StreamRequest, Response>
            {
                public IAsyncEnumerable<Response> Handle(
                    StreamRequest request,
                    StreamHandlerDelegate<StreamRequest, Response> next,
                    CancellationToken cancellationToken
                ) => next(request, cancellationToken);
            }

            public sealed class RequestHandler : IRequestHandler<Request, Response>, IStreamRequestHandler<StreamRequest, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));

                public async IAsyncEnumerable<Response> Handle(
                    StreamRequest request,
                    [EnumeratorCancellation] CancellationToken cancellationToken
                )
                {
                    for (var i = 0; i < 3; i++)
                    {
                        await Task.Delay(10, cancellationToken);
                        yield return new Response(request.Id);
                    }
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

    [Fact]
    public async Task Test_PipelineBehaviors_Invalid_Types()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using System.Collections.Generic;
            using System.Runtime.CompilerServices;
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
                            typeof(GenericBehavior<,>),
                            typeof(object),
                        ];
                        options.StreamPipelineBehaviors =
                        [
                            typeof(StreamGenericBehavior<,>),
                            typeof(object),
                        ];
                    });
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;

            public readonly record struct Response(Guid Id);

            public readonly record struct StreamRequest(Guid Id) : IStreamRequest<Response>;

            public sealed class GenericBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
                where TMessage : IMessage
            {
                public ValueTask<TResponse> Handle(
                    TMessage message,
                    MessageHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }


            public sealed class StreamGenericBehavior<TMessage, TResponse> : IStreamPipelineBehavior<TMessage, TResponse>
                where TMessage : IStreamMessage
            {
                public IAsyncEnumerable<TResponse> Handle(
                    TMessage message,
                    StreamHandlerDelegate<TMessage, TResponse> next,
                    CancellationToken cancellationToken
                ) => next(message, cancellationToken);
            }

            public sealed class RequestHandler : IRequestHandler<Request, Response>, IStreamRequestHandler<StreamRequest, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new ValueTask<Response>(new Response(request.Id));

                public async IAsyncEnumerable<Response> Handle(
                    StreamRequest request,
                    [EnumeratorCancellation] CancellationToken cancellationToken
                )
                {
                    for (var i = 0; i < 3; i++)
                    {
                        await Task.Delay(10, cancellationToken);
                        yield return new Response(request.Id);
                    }
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify();
    }

    [Theory]
    [CombinatorialData]
    public async Task Test_Telemetry_Matrix_AllMessageKinds(
        ServiceLifetime sl,
        [CombinatorialValues("Eager", "Lazy")] string cm,
        bool em,
        bool et,
        bool oi
    )
    {
        var enableMetricsLiteral = em.ToString().ToLowerInvariant();
        var enableTracingLiteral = et.ToString().ToLowerInvariant();
        var telemetryConfig = oi
            ? $$"""
                          options.Telemetry = new()
                          {
                              EnableMetrics = {{enableMetricsLiteral}},
                              MeterName = "TestMeter",
                              EnableTracing = {{enableTracingLiteral}},
                              ActivitySourceName = "TestActivitySource"
                          };
                """
            : $$"""
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
                        options.ServiceLifetime = ServiceLifetime.{{sl}};
                        options.CachingMode = CachingMode.{{cm}};
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
            .UseParameters(
                sl == ServiceLifetime.Singleton ? "S"
                    : sl == ServiceLifetime.Scoped ? "Sc"
                    : "T",
                cm == "Lazy" ? "L" : "E",
                em ? "1" : "0",
                et ? "1" : "0",
                oi ? "1" : "0"
            );
    }

    [Theory]
    [CombinatorialData]
    public async Task Test_Telemetry_Attribute_Matrix(bool em, bool am)
    {
        var enableMetricsLiteral = em.ToString().ToLowerInvariant();
        var expectedMeterName = am ? "TelemetryAttrMeterA" : "TelemetryAttrMeterB";
        var meterNameAssignment = $"TelemetryMeterName = \"{expectedMeterName}\"";

        var inputCompilation = Fixture.CreateLibrary(
            $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            [assembly: MediatorOptions(TelemetryEnableMetrics = {{enableMetricsLiteral}}, {{meterNameAssignment}})]

            namespace TestCode;

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

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

        await inputCompilation
            .AssertAndVerify(Assertions.CompilesWithoutDiagnostics)
            .UseParameters(em ? "1" : "0", am ? "A" : "B");
    }

    [Theory]
    [CombinatorialData]
    public async Task Test_Telemetry_Metrics_Code_Config_Parses_Into_Model(bool oi)
    {
        var telemetryConfig = oi
            ? """
                          options.Telemetry = new()
                          {
                              EnableMetrics = true,
                              MeterName = "CodeMeter"
                          };
                """
            : """
                          options.Telemetry.EnableMetrics = true;
                          options.Telemetry.MeterName = "CodeMeter";
                """;

        var inputCompilation = Fixture.CreateLibrary(
            $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    services.AddMediator(options =>
                    {
            {{telemetryConfig}}
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
                Assert.True(model.EnableMetrics);
                Assert.Equal("CodeMeter", model.MeterName);
                Assert.False(model.ConfiguredViaAttribute);
            }
        );
    }

    [Fact]
    public async Task Test_Telemetry_Metrics_Attribute_Config_Parses_Into_Model()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            [assembly: MediatorOptions(TelemetryEnableMetrics = true, TelemetryMeterName = "AttributeMeter")]

            namespace TestCode;

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

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
                Assert.True(model.EnableMetrics);
                Assert.Equal("AttributeMeter", model.MeterName);
                Assert.True(model.ConfiguredViaAttribute);
            }
        );
    }

    [Fact]
    public async Task Test_Telemetry_Metrics_Code_Config_Rejects_NonLiteral_EnableMetrics_Assignment()
    {
        await AssertMetricsCodeConfigRejectsNonLiteralAssignment(
            optionName: "EnableMetrics",
            valueExpression: "GetMetricsEnabled()",
            helperMethod: "private static bool GetMetricsEnabled() => true;",
            expectedMessage: "Expected boolean literal for 'EnableMetrics'"
        );
    }

    [Fact]
    public async Task Test_Telemetry_Metrics_Code_Config_Rejects_NonLiteral_MeterName_Assignment()
    {
        await AssertMetricsCodeConfigRejectsNonLiteralAssignment(
            optionName: "MeterName",
            valueExpression: "GetMetricsMeterName()",
            helperMethod: "private static string GetMetricsMeterName() => \"CodeMeter\";",
            expectedMessage: "Expected string literal for 'MeterName'"
        );
    }

    [Fact]
    public async Task Test_Telemetry_Metrics_Code_Config_Rejects_NonLiteral_HistogramBuckets_Assignment()
    {
        await AssertMetricsCodeConfigRejectsNonLiteralAssignment(
            optionName: "HistogramBuckets",
            valueExpression: "GetHistogramBuckets()",
            helperMethod: "private static double[] GetHistogramBuckets() => new[] { 0.1d, 0.5d, 1.0d };",
            expectedMessage: "Expected array creation or null for 'HistogramBuckets'"
        );
    }

    private async Task AssertMetricsCodeConfigRejectsNonLiteralAssignment(
        string optionName,
        string valueExpression,
        string helperMethod,
        string expectedMessage
    )
    {
        var inputCompilation = Fixture.CreateLibrary(
            $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    services.AddMediator(options =>
                    {
                        options.Telemetry.{{optionName}} = {{valueExpression}};
                    });
                }

                {{helperMethod}}
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            (Action<GeneratorResult>)(
                result =>
                {
                    Assertions.AssertCommon(result);

                    var diagnostics = result.Diagnostics.Where(d =>
                        d.Id == Diagnostics.InvalidCodeBasedConfiguration.Id
                    );
                    Assert.Contains(diagnostics, d => d.GetMessage().Contains(expectedMessage));
                }
            )
        );
    }

    [Theory]
    [CombinatorialData]
    public async Task Test_Telemetry_Tracing_Code_Config_Parses_Into_Model(bool oi)
    {
        var telemetryConfig = oi
            ? """
                          options.Telemetry = new()
                          {
                              EnableTracing = true,
                              ActivitySourceName = "CodeTracingSource"
                          };
                """
            : """
                          options.Telemetry.EnableTracing = true;
                          options.Telemetry.ActivitySourceName = "CodeTracingSource";
                """;

        var inputCompilation = Fixture.CreateLibrary(
            $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    services.AddMediator(options =>
                    {
            {{telemetryConfig}}
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
                Assert.True(model.EnableTracing);
                Assert.Equal("CodeTracingSource", model.ActivitySourceName);
                Assert.False(model.ConfiguredViaAttribute);
            }
        );
    }

    [Fact]
    public async Task Test_Telemetry_Tracing_Attribute_Config_Parses_Into_Model()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            [assembly: MediatorOptions(TelemetryEnableTracing = true, TelemetryActivitySourceName = "AttributeTracingSource")]

            namespace TestCode;

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

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
                Assert.True(model.EnableTracing);
                Assert.Equal("AttributeTracingSource", model.ActivitySourceName);
                Assert.True(model.ConfiguredViaAttribute);
            }
        );
    }

    [Fact]
    public async Task Test_Telemetry_Tracing_Code_Config_Rejects_NonLiteral_EnableTracing_Assignment()
    {
        await AssertTracingCodeConfigRejectsNonLiteralAssignment(
            optionName: "EnableTracing",
            valueExpression: "GetTracingEnabled()",
            helperMethod: "private static bool GetTracingEnabled() => true;",
            expectedMessage: "Expected boolean literal for 'EnableTracing'"
        );
    }

    [Fact]
    public async Task Test_Telemetry_Tracing_Code_Config_Rejects_NonLiteral_ActivitySourceName_Assignment()
    {
        await AssertTracingCodeConfigRejectsNonLiteralAssignment(
            optionName: "ActivitySourceName",
            valueExpression: "GetTracingSourceName()",
            helperMethod: "private static string GetTracingSourceName() => \"CodeTracingSource\";",
            expectedMessage: "Expected string literal for 'ActivitySourceName'"
        );
    }

    private async Task AssertTracingCodeConfigRejectsNonLiteralAssignment(
        string optionName,
        string valueExpression,
        string helperMethod,
        string expectedMessage
    )
    {
        var inputCompilation = Fixture.CreateLibrary(
            $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);

            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    services.AddMediator(options =>
                    {
                        options.Telemetry.{{optionName}} = {{valueExpression}};
                    });
                }

                {{helperMethod}}
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            (Action<GeneratorResult>)(
                result =>
                {
                    Assertions.AssertCommon(result);

                    var diagnostics = result.Diagnostics.Where(d =>
                        d.Id == Diagnostics.InvalidCodeBasedConfiguration.Id
                    );
                    Assert.Contains(diagnostics, d => d.GetMessage().Contains(expectedMessage));
                }
            )
        );
    }

    [Fact]
    public async Task Test_Telemetry_Detects_MeterProvider_Extensions_Package()
    {
        var openTelemetryProviderBuilderExtensions = Fixture
            .CreateLibrary(
                """
                using System;
                using Microsoft.Extensions.DependencyInjection;

                namespace OpenTelemetry.Metrics;

                public class MeterProviderBuilder
                {
                    public MeterProviderBuilder AddMeter(string meterName) => this;
                }

                public static class OpenTelemetryDependencyInjectionMetricsServiceCollectionExtensions
                {
                    public static IServiceCollection ConfigureOpenTelemetryMeterProvider(
                        this IServiceCollection services,
                        Action<MeterProviderBuilder> configure)
                    {
                        var builder = new MeterProviderBuilder();
                        configure(builder);
                        return services;
                    }
                }
                """
            )
            .WithAssemblyName("OpenTelemetry.Api.ProviderBuilderExtensions");

        var inputCompilation = Fixture
            .CreateLibrary(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Mediator;
                using Microsoft.Extensions.DependencyInjection;

                namespace TestCode;

                public readonly record struct Request(Guid Id) : IRequest<Response>;
                public readonly record struct Response(Guid Id);

                public sealed class RequestHandler : IRequestHandler<Request, Response>
                {
                    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                        new(new Response(request.Id));
                }

                public class Program
                {
                    public static void Main()
                    {
                        var services = new ServiceCollection();
                        services.AddMediator(options =>
                        {
                            options.Telemetry.EnableMetrics = true;
                            options.Telemetry.MeterName = "TestMeter";
                        });
                    }
                }
                """
            )
            .AddReferences(openTelemetryProviderBuilderExtensions.ToMetadataReference());

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Telemetry_Detects_TracerProvider_Extensions_Package()
    {
        var openTelemetryProviderBuilderExtensions = Fixture
            .CreateLibrary(
                """
                using System;
                using Microsoft.Extensions.DependencyInjection;

                namespace OpenTelemetry.Trace;

                public class TracerProviderBuilder
                {
                    public TracerProviderBuilder AddSource(string sourceName) => this;
                }

                public static class OpenTelemetryDependencyInjectionTracingServiceCollectionExtensions
                {
                    public static IServiceCollection ConfigureOpenTelemetryTracerProvider(
                        this IServiceCollection services,
                        Action<TracerProviderBuilder> configure)
                    {
                        var builder = new TracerProviderBuilder();
                        configure(builder);
                        return services;
                    }
                }
                """
            )
            .WithAssemblyName("OpenTelemetry.Api.ProviderBuilderExtensions");

        var inputCompilation = Fixture
            .CreateLibrary(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Mediator;
                using Microsoft.Extensions.DependencyInjection;

                namespace TestCode;

                public readonly record struct Request(Guid Id) : IRequest<Response>;
                public readonly record struct Response(Guid Id);

                public sealed class RequestHandler : IRequestHandler<Request, Response>
                {
                    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                        new(new Response(request.Id));
                }

                public class Program
                {
                    public static void Main()
                    {
                        var services = new ServiceCollection();
                        services.AddMediator(options =>
                        {
                            options.Telemetry.EnableTracing = true;
                            options.Telemetry.ActivitySourceName = "TestActivitySource";
                        });
                    }
                }
                """
            )
            .AddReferences(openTelemetryProviderBuilderExtensions.ToMetadataReference());

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }
}
