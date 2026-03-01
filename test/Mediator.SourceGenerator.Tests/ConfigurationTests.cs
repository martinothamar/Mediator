using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    public async Task Test_Telemetry_Exposes_MeterName_On_Mediator_When_Metrics_Enabled()
    {
        var inputCompilation = Fixture.CreateLibrary(
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

                    var meterName = global::Mediator.Mediator.MeterName;
                    if (meterName != "TestMeter")
                        throw new Exception("Unexpected meter name");
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Telemetry_Exposes_ActivitySourceName_On_Mediator_When_Tracing_Enabled()
    {
        var inputCompilation = Fixture.CreateLibrary(
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

                    var activitySourceName = global::Mediator.Mediator.ActivitySourceName;
                    if (activitySourceName != "TestActivitySource")
                        throw new Exception("Unexpected activity source name");
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public void Test_Telemetry_Tracing_Disabled_When_ActivitySource_Symbol_Is_Unavailable()
    {
        var inputCompilation = CreateLibraryWithoutDiagnosticSourceReference(
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
        );

        var result = RunGenerator(inputCompilation);
        Assertions.AssertCommon(result);
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(
            result
                .OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
        );
        Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.TracingUnavailableOnTarget.Id);
        Assert.All(
            result.Diagnostics.Where(d => d.Id == Diagnostics.TracingUnavailableOnTarget.Id),
            d => Assert.True(d.Location.IsInSource)
        );

        var model = result.Generator.CompilationModel;
        Assert.NotNull(model);
        Assert.True(model.EnableTracing);
        Assert.False(model.EnableTracingOnTarget);
        Assert.Equal("global::Mediator.ForeachAwaitPublisher", model.NotificationPublisherResolvedTypeFullName);
    }

    [Fact]
    public void Test_Telemetry_Tracing_Attribute_Config_Disabled_When_ActivitySource_Symbol_Is_Unavailable()
    {
        var inputCompilation = CreateLibraryWithoutDiagnosticSourceReference(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            [assembly: MediatorOptions(TelemetryEnableTracing = true, TelemetryActivitySourceName = "AttrTracingSource")]

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

        var result = RunGenerator(inputCompilation);
        Assertions.AssertCommon(result);
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(
            result
                .OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
        );
        Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.TracingUnavailableOnTarget.Id);
        Assert.All(
            result.Diagnostics.Where(d => d.Id == Diagnostics.TracingUnavailableOnTarget.Id),
            d => Assert.True(d.Location.IsInSource)
        );

        var model = result.Generator.CompilationModel;
        Assert.NotNull(model);
        Assert.True(model.EnableTracing);
        Assert.True(model.ConfiguredViaAttribute);
        Assert.False(model.EnableTracingOnTarget);
        Assert.Equal("global::Mediator.ForeachAwaitPublisher", model.NotificationPublisherResolvedTypeFullName);
    }

    [Fact]
    public void Test_Telemetry_Metrics_Disabled_When_Meter_Symbol_Is_Unavailable()
    {
        var inputCompilation = CreateLibraryWithoutDiagnosticSourceReference(
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
        );

        var result = RunGenerator(inputCompilation);
        Assertions.AssertCommon(result);
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(
            result
                .OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
        );
        Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MetricsUnavailableOnTarget.Id);
        Assert.All(
            result.Diagnostics.Where(d => d.Id == Diagnostics.MetricsUnavailableOnTarget.Id),
            d => Assert.True(d.Location.IsInSource)
        );

        var model = result.Generator.CompilationModel;
        Assert.NotNull(model);
        Assert.True(model.EnableMetrics);
        Assert.False(model.EnableMetricsOnTarget);
        Assert.Equal("global::Mediator.ForeachAwaitPublisher", model.NotificationPublisherResolvedTypeFullName);
    }

    [Fact]
    public void Test_Telemetry_Metrics_Attribute_Config_Disabled_When_Meter_Symbol_Is_Unavailable()
    {
        var inputCompilation = CreateLibraryWithoutDiagnosticSourceReference(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            [assembly: MediatorOptions(TelemetryEnableMetrics = true, TelemetryMeterName = "AttrMeter")]

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

        var result = RunGenerator(inputCompilation);
        Assertions.AssertCommon(result);
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(
            result
                .OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
        );
        Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MetricsUnavailableOnTarget.Id);
        Assert.All(
            result.Diagnostics.Where(d => d.Id == Diagnostics.MetricsUnavailableOnTarget.Id),
            d => Assert.True(d.Location.IsInSource)
        );

        var model = result.Generator.CompilationModel;
        Assert.NotNull(model);
        Assert.True(model.EnableMetrics);
        Assert.True(model.ConfiguredViaAttribute);
        Assert.False(model.EnableMetricsOnTarget);
        Assert.Equal("global::Mediator.ForeachAwaitPublisher", model.NotificationPublisherResolvedTypeFullName);
    }

    [Fact]
    public void Test_Telemetry_Tracing_Only_On_Target_When_TargetFramework_Symbols_Are_Missing()
    {
        var inputCompilation = CreateLibraryWithoutTargetFrameworkPreprocessorSymbols(
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
                        options.Telemetry.EnableTracing = true;
                        options.Telemetry.ActivitySourceName = "TestActivitySource";
                    });
                }
            }
            """
        );

        var result = RunGenerator(inputCompilation);
        Assertions.AssertCommon(result);
        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(
            result
                .OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
        );
        Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MetricsUnavailableOnTarget.Id);
        Assert.All(
            result.Diagnostics.Where(d => d.Id == Diagnostics.MetricsUnavailableOnTarget.Id),
            d => Assert.True(d.Location.IsInSource)
        );
        Assert.DoesNotContain(result.Diagnostics, d => d.Id == Diagnostics.TracingUnavailableOnTarget.Id);

        var model = result.Generator.CompilationModel;
        Assert.NotNull(model);
        Assert.False(model.TargetFrameworkIsNet8OrGreater);
        Assert.True(model.EnableMetrics);
        Assert.True(model.EnableTracing);
        Assert.False(model.EnableMetricsOnTarget);
        Assert.True(model.EnableTracingOnTarget);
        Assert.Equal(
            $"global::{model.InternalsNamespace}.MediatorTelemetryNotificationPublisher",
            model.NotificationPublisherResolvedTypeFullName
        );
    }

    private static CSharpCompilation CreateLibraryWithoutDiagnosticSourceReference(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            CSharpParseOptions.Default.WithPreprocessorSymbols(
                "NET8_0_OR_GREATER",
                "NET9_0_OR_GREATER",
                "NET10_0_OR_GREATER"
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var diagnosticSourceAssemblyLocation = typeof(System.Diagnostics.ActivitySource).Assembly.Location;
        var references = Fixture
            .AssemblyReferencesForCodegen.Where(a =>
                !a.IsDynamic && !string.Equals(a.Location, diagnosticSourceAssemblyLocation, StringComparison.Ordinal)
            )
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        return CSharpCompilation.Create(
            "Library",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private static CSharpCompilation CreateLibraryWithoutTargetFrameworkPreprocessorSymbols(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);

        var references = Fixture
            .AssemblyReferencesForCodegen.Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        return CSharpCompilation.Create(
            "Library",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private static GeneratorResult RunGenerator(CSharpCompilation inputCompilation)
    {
        var generator = new IncrementalMediatorGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            inputCompilation,
            out var outputCompilation,
            out var diagnostics,
            cancellationToken: TestContext.Current.CancellationToken
        );
        var runResult = driver.GetRunResult();

        return new(generator, diagnostics, runResult, outputCompilation);
    }
}
