using System.Linq;
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

    [Fact]
    public async Task Test_IncludeTypesForGeneration_SingleType()
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
                        options.IncludeTypesForGeneration =
                        [
                            typeof(IApi1Request),
                        ];
                    });
                }
            }

            public interface IApi1Request { }

            public readonly record struct Request1(Guid Id) : IRequest<Response1>, IApi1Request;
            public readonly record struct Response1(Guid Id);
            public sealed class Request1Handler : IRequestHandler<Request1, Response1>
            {
                public ValueTask<Response1> Handle(Request1 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response1>(new Response1(request.Id));
            }

            public readonly record struct Request2(Guid Id) : IRequest<Response2>;
            public readonly record struct Response2(Guid Id);
            public sealed class Request2Handler : IRequestHandler<Request2, Response2>
            {
                public ValueTask<Response2> Handle(Request2 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response2>(new Response2(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                // Should only have Request1 (implements IApi1Request), not Request2
                Assert.Single(model.RequestMessages);
                Assert.Equal("Request1", model.RequestMessages[0].Name);
            }
        );
    }

    [Fact]
    public async Task Test_IncludeTypesForGeneration_MultipleTypes()
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
                        options.IncludeTypesForGeneration =
                        [
                            typeof(IApi1Request),
                            typeof(IApi2Request),
                        ];
                    });
                }
            }

            public interface IApi1Request { }
            public interface IApi2Request { }

            public readonly record struct Request1(Guid Id) : IRequest<Response1>, IApi1Request;
            public readonly record struct Response1(Guid Id);
            public sealed class Request1Handler : IRequestHandler<Request1, Response1>
            {
                public ValueTask<Response1> Handle(Request1 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response1>(new Response1(request.Id));
            }

            public readonly record struct Request2(Guid Id) : IRequest<Response2>, IApi2Request;
            public readonly record struct Response2(Guid Id);
            public sealed class Request2Handler : IRequestHandler<Request2, Response2>
            {
                public ValueTask<Response2> Handle(Request2 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response2>(new Response2(request.Id));
            }

            public readonly record struct Request3(Guid Id) : IRequest<Response3>, IApi1Request, IApi2Request;
            public readonly record struct Response3(Guid Id);
            public sealed class Request3Handler : IRequestHandler<Request3, Response3>
            {
                public ValueTask<Response3> Handle(Request3 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response3>(new Response3(request.Id));
            }

            public readonly record struct Request4(Guid Id) : IRequest<Response4>;
            public readonly record struct Response4(Guid Id);
            public sealed class Request4Handler : IRequestHandler<Request4, Response4>
            {
                public ValueTask<Response4> Handle(Request4 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response4>(new Response4(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                // Should have Request1, Request2, and Request3 (all implement marker interfaces)
                // Should NOT have Request4 (doesn't implement any marker interface)
                Assert.Equal(3, model.RequestMessages.Count);
                var requestNames = new[]
                {
                    model.RequestMessages[0].Name,
                    model.RequestMessages[1].Name,
                    model.RequestMessages[2].Name,
                }
                    .OrderBy(n => n)
                    .ToArray();
                Assert.Equal(new[] { "Request1", "Request2", "Request3" }, requestNames);
            }
        );
    }

    [Fact]
    public async Task Test_IncludeTypesForGeneration_EmptyList()
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
                        options.IncludeTypesForGeneration = [];
                    });
                }
            }

            public readonly record struct Request1(Guid Id) : IRequest<Response1>;
            public readonly record struct Response1(Guid Id);
            public sealed class Request1Handler : IRequestHandler<Request1, Response1>
            {
                public ValueTask<Response1> Handle(Request1 request, CancellationToken cancellationToken) =>
                    new ValueTask<Response1>(new Response1(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                // Empty filter list means no filtering - all requests should be included
                Assert.Single(model.RequestMessages);
                Assert.Equal("Request1", model.RequestMessages[0].Name);
            }
        );
    }

    [Fact]
    public async Task Test_IncludeTypesForGeneration_ModularMonolith_TwoAPIs()
    {
        // This test demonstrates a modular monolith scenario with two APIs
        // sharing the same codebase but only generating mediator code for their respective requests

        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            // Marker interfaces for different APIs/modules
            public interface IApi1Request { }
            public interface IApi2Request { }

            public class Api1Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    // API1 only generates code for IApi1Request handlers
                    services.AddMediator(options =>
                    {
                        options.Namespace = "Api1";
                        options.IncludeTypesForGeneration = [typeof(IApi1Request)];
                    });
                }
            }

            // Shared across both APIs
            public readonly record struct SharedRequest(Guid Id) : IRequest<SharedResponse>, IApi1Request, IApi2Request;
            public readonly record struct SharedResponse(Guid Id);
            public sealed class SharedRequestHandler : IRequestHandler<SharedRequest, SharedResponse>
            {
                public ValueTask<SharedResponse> Handle(SharedRequest request, CancellationToken cancellationToken) =>
                    new ValueTask<SharedResponse>(new SharedResponse(request.Id));
            }

            // API1 only
            public readonly record struct Api1OnlyRequest(Guid Id) : IRequest<Api1Response>, IApi1Request;
            public readonly record struct Api1Response(Guid Id);
            public sealed class Api1OnlyRequestHandler : IRequestHandler<Api1OnlyRequest, Api1Response>
            {
                public ValueTask<Api1Response> Handle(Api1OnlyRequest request, CancellationToken cancellationToken) =>
                    new ValueTask<Api1Response>(new Api1Response(request.Id));
            }

            // API2 only - should NOT be included in API1's generated code
            public readonly record struct Api2OnlyRequest(Guid Id) : IRequest<Api2Response>, IApi2Request;
            public readonly record struct Api2Response(Guid Id);
            public sealed class Api2OnlyRequestHandler : IRequestHandler<Api2OnlyRequest, Api2Response>
            {
                public ValueTask<Api2Response> Handle(Api2OnlyRequest request, CancellationToken cancellationToken) =>
                    new ValueTask<Api2Response>(new Api2Response(request.Id));
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                // Should only have SharedRequest and Api1OnlyRequest
                // Should NOT have Api2OnlyRequest
                Assert.Equal(2, model.RequestMessages.Count);
                var requestNames = new[] { model.RequestMessages[0].Name, model.RequestMessages[1].Name }
                    .OrderBy(n => n)
                    .ToArray();
                Assert.Equal(new[] { "Api1OnlyRequest", "SharedRequest" }, requestNames);
            }
        );
    }
}
