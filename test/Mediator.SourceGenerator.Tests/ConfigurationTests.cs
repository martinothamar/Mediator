using System.Threading.Tasks;

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
}
