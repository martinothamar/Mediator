using Mediator.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediator.Benchmarks.SourceGenerator;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[InProcess]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class SourceGeneratorBenchmark
{
    static readonly Assembly[] ImportantAssemblies = new[]
    {
        typeof(object).Assembly,
        typeof(IMessage).Assembly,
        typeof(ServiceLifetime).Assembly,
        typeof(ServiceProvider).Assembly,
        typeof(MulticastDelegate).Assembly
    };

    private Compilation _compilation;

    private CSharpGeneratorDriver _driver;

    static Assembly[] AssemblyReferencesForCodegen =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .Concat(ImportantAssemblies)
            .Distinct()
            .Where(a => !a.IsDynamic)
            .ToArray();

    static Compilation CreateLibrary(params string[] source)
    {
        var references = new List<MetadataReference>();
        var assemblies = AssemblyReferencesForCodegen;
        foreach (Assembly assembly in assemblies)
        {
            if (!assembly.IsDynamic)
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        var compilation = CSharpCompilation.Create(
            "compilation",
            source.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray(),
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return compilation;
    }

    [GlobalSetup]
    public void Setup()
    {
        _compilation = CreateLibrary(_code);

        var generator = new MediatorGenerator();

        _driver = CSharpGeneratorDriver.Create(generator);
    }

    [Benchmark]
    public GeneratorDriver Compile()
    {
        return _driver.RunGenerators(_compilation);
    }

    private static readonly string _code =
        @"
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

var services = new ServiceCollection();

services.AddMediator(
    options =>
    {
        options.Namespace = null;
        options.ServiceLifetime = ServiceLifetime.Singleton;
    }
);

// Standard handlers are added by default, but we need to add pipeline steps manually.
// Here are two examples.
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericLoggerHandler<,>)); // This will run 1st
services.AddSingleton<IPipelineBehavior<Ping, Pong>, PingValidator>(); // This will run 2nd

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

var id = Guid.NewGuid();
var request = new Ping(id);

var response = await mediator.Send(request);

return response.Id == id ? 0 : 1;

//
// Here are the types used
//

public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class GenericLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next
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
        CancellationToken cancellationToken,
        MessageHandlerDelegate<Ping, Pong> next
    )
    {
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


public sealed record Pinged(Ping Ping) : INotification;

public sealed class AllNotificationsHandler : INotificationHandler<INotification>
{
    public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
    {
        return default;
    }
}

public sealed record IntegrationEvent<T>(T Message) : INotification
    where T : IMessage;

public sealed class PingedHandler : INotificationHandler<Pinged>
{
    public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
    {
        return default;
    }
}
    ";
}
