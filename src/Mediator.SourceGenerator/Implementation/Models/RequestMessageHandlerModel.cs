using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerModel : SymbolMetadataModel
{
    public string MessageType { get; }
    public RequestMessageHandlerWrapperModel WrapperType { get; }

    public RequestMessageHandlerModel(
        INamedTypeSymbol symbol,
        string messageType,
        CompilationAnalyzer analyzer,
        RequestMessageHandlerWrapperModel wrapperType
    )
        : base(symbol)
    {
        MessageType = messageType;
        WrapperType = wrapperType;

        var sd = "global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor";
        var typeOfExpression = $"typeof({symbol.GetTypeSymbolFullName()})";
        ServiceRegistration =
            $"services.TryAdd(new {sd}({typeOfExpression}, {typeOfExpression}, {analyzer.ServiceLifetime}));";
    }

    public string ServiceRegistration { get; }
}
