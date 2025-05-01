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
