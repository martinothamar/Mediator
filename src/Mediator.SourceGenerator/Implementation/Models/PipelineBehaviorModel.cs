using System.Text;
using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record PipelineBehaviorModel : SymbolMetadataModel
{
    public PipelineBehaviorModel(PipelineBehaviorType type, CompilationAnalyzer analyzer)
        : base(type.Symbol)
    {
        ServiceRegistrationBlock = "";
        if (type.Messages.Count > 0)
        {
            var interfaceSymbol = type.InterfaceSymbol.GetTypeSymbolFullName(includeTypeParameters: false);
            var concreteSymbol = type.Symbol.GetTypeSymbolFullName(includeTypeParameters: false);
            var builder = new StringBuilder();
            foreach (var message in type.Messages)
            {
                var requestType = message.Symbol.GetTypeSymbolFullName();
                var responseType = message.ResponseSymbol.GetTypeSymbolFullName();
                builder.AppendLine(
                    $"services.Add(new SD(typeof({interfaceSymbol}<{requestType}, {responseType}>), typeof({concreteSymbol}{(type.Symbol.IsGenericType ? $"<{requestType}, {responseType}>" : "")}), {analyzer.ServiceLifetime}));"
                );
            }

            ServiceRegistrationBlock = builder.ToString();
        }
    }

    public string ServiceRegistrationBlock { get; }
}
