using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record RequestMessageHandlerModel
{
    public string MessageType { get; }
    public RequestMessageHandlerWrapperModel WrapperType { get; }

    public RequestMessageHandlerModel(
        INamedTypeSymbol symbol,
        string messageType,
        CompilationAnalyzer analyzer,
        RequestMessageHandlerWrapperModel wrapperType
    )
    {
        MessageType = messageType;
        WrapperType = wrapperType;

        var typeOfExpression = $"typeof({symbol.GetTypeSymbolFullName()})";
        ServiceRegistration =
            $"services.TryAdd(new SD({typeOfExpression}, {typeOfExpression}, {analyzer.ServiceLifetime}));";
    }

    public string ServiceRegistration { get; }
}
