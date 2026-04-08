using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mediator.SourceGenerator.Tests;

public sealed class ModularRegistrationTests
{
    [Fact]
    public void AddMediatorHandlers_UsesIdempotentRegistrations()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public static class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();
                    services.AddMediatorHandlers();
                }
            }

            public readonly record struct Request(Guid Id) : IRequest<Response>;
            public readonly record struct Response(Guid Id);
            public sealed class RequestHandler : IRequestHandler<Request, Response>
            {
                public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                    new(new Response(request.Id));
            }

            public sealed record Notification(Guid Id) : INotification;
            public sealed class NotificationHandler : INotificationHandler<Notification>
            {
                public ValueTask Handle(Notification notification, CancellationToken cancellationToken) => default;
            }
            """
        );

        var result = RunGenerator(inputCompilation);

        result.Diagnostics.Should().BeEmpty();

        var source = GetMediatorSource(result);
        source
            .Should()
            .Contain(
                "services.TryAdd(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(global::Mediator.IRequestHandler<global::TestCode.Request, global::TestCode.Response>), typeof(global::TestCode.RequestHandler), global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));"
            );
        source
            .Should()
            .Contain(
                "services.TryAddEnumerable(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(global::Mediator.INotificationHandler<global::TestCode.Notification>), GetRequiredService<global::TestCode.NotificationHandler>(), global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));"
            );
    }

    [Fact]
    public void AddMediatorHandlers_RegistersInternalsVisibleToHandlers()
    {
        var sharedCompilation = Fixture
            .CreateLibrary(
                """
                using System;
                using System.Runtime.CompilerServices;
                using System.Threading;
                using System.Threading.Tasks;
                using Mediator;

                [assembly: InternalsVisibleTo("TestCode.Api")]

                namespace TestCode.Shared;

                public readonly record struct Request(Guid Id) : IRequest<Response>;
                public readonly record struct Response(Guid Id);
                public sealed record Notification(Guid Id) : INotification;

                internal sealed class RequestHandler : IRequestHandler<Request, Response>
                {
                    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
                        new(new Response(request.Id));
                }

                internal sealed class NotificationHandler : INotificationHandler<Notification>
                {
                    public ValueTask Handle(Notification notification, CancellationToken cancellationToken) => default;
                }
                """
            )
            .WithAssemblyName("TestCode.Shared");

        var inputCompilation = Fixture
            .CreateLibrary(
                """
                using Mediator;
                using Microsoft.Extensions.DependencyInjection;
                using TestCode.Shared;

                namespace TestCode.Api;

                public static class Program
                {
                    public static void Main()
                    {
                        var services = new ServiceCollection();
                        services.AddMediatorHandlers(options =>
                        {
                            options.Assemblies = [typeof(Request)];
                        });
                    }
                }
                """
            )
            .WithAssemblyName("TestCode.Api")
            .AddReferences(ToMetadataReference(sharedCompilation));

        var result = RunGenerator(inputCompilation);

        result.Diagnostics.Should().BeEmpty();
        result
            .OutputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should()
            .BeEmpty();

        var source = GetMediatorSource(result);
        source
            .Should()
            .Contain(
                "services.TryAdd(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(global::Mediator.IRequestHandler<global::TestCode.Shared.Request, global::TestCode.Shared.Response>), typeof(global::TestCode.Shared.RequestHandler), global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));"
            );
        source
            .Should()
            .Contain(
                "services.TryAddEnumerable(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(global::Mediator.INotificationHandler<global::TestCode.Shared.Notification>), GetRequiredService<global::TestCode.Shared.NotificationHandler>(), global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));"
            );
    }

    private static string GetMediatorSource(GeneratorResult result)
    {
        return result
            .RunResult.Results.SelectMany(x => x.GeneratedSources)
            .Single(x => x.HintName == "Mediator.g.cs")
            .SourceText.ToString();
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

    private static MetadataReference ToMetadataReference(Compilation compilation)
    {
        using var peStream = new MemoryStream();
        var emitResult = compilation.Emit(peStream, cancellationToken: TestContext.Current.CancellationToken);
        emitResult.Success.Should().BeTrue();
        return MetadataReference.CreateFromImage(peStream.ToArray());
    }
}
