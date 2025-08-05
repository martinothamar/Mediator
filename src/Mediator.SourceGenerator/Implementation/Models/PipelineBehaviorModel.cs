using System.Text;
using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record PipelineBehaviorModel : SymbolMetadataModel
{
    public PipelineBehaviorModel(PipelineBehaviorType type, CompilationAnalyzer analyzer)
        : base(type.Symbol)
    {
        ServiceRegistrations = ImmutableEquatableArray<string>.Empty;
        if (type.Messages.Count > 0)
        {
            var sd = "global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor";
            var interfaceSymbol = type.InterfaceSymbol.GetTypeSymbolFullName(includeTypeParameters: false);
            var concreteSymbol = type.Symbol.GetTypeSymbolFullName(includeTypeParameters: false);
            var builder = new List<string>(type.Messages.Count);
            foreach (var message in type.Messages)
            {
                var requestType = message.Symbol.GetTypeSymbolFullName();
                var responseType = message.ResponseSymbol.GetTypeSymbolFullName();
                var registration =
                    $"services.Add(new {sd}(typeof({interfaceSymbol}<{requestType}, {responseType}>), typeof({concreteSymbol}{(type.Symbol.IsGenericType ? $"<{requestType}, {responseType}>" : "")}), {analyzer.ServiceLifetime}));";
                builder.Add(registration);
            }

            ServiceRegistrations = builder.ToImmutableEquatableArray();
        }
    }

    public ImmutableEquatableArray<string> ServiceRegistrations { get; }
}
