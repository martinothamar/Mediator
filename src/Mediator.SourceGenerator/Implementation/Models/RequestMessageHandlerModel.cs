using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerModel : SymbolMetadataModel
{
    public string MessageType { get; }

    public RequestMessageHandlerModel(
        INamedTypeSymbol symbol,
        INamedTypeSymbol messageSymbol,
        ITypeSymbol responseSymbol,
        string messageType,
        CompilationAnalyzer analyzer
    )
        : base(symbol)
    {
        MessageType = messageType;

        if (!symbol.IsAccessibleFromGeneratedCode(analyzer.Compilation))
        {
            ServiceRegistration = string.Empty;
            return;
        }

        var sd = "global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor";
        var interfaceType =
            $"typeof(global::Mediator.I{messageType}Handler<{messageSymbol.GetTypeSymbolFullName()}, {responseSymbol.GetTypeSymbolFullName()}>)";
        var typeOfExpression = $"typeof({symbol.GetTypeSymbolFullName()})";
        ServiceRegistration =
            $"services.TryAdd(new {sd}({interfaceType}, {typeOfExpression}, {analyzer.ServiceLifetime}));";
    }

    public string ServiceRegistration { get; }
}
