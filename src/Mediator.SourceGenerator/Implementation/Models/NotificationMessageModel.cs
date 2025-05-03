using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record NotificationMessageModel : SymbolMetadataModel
{
    public NotificationMessageModel(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
        : base(symbol)
    {
        var identifierFullName = symbol
            .GetTypeSymbolFullName(withGlobalPrefix: false, includeTypeParameters: false)
            .Replace("global::", "")
            .Replace('.', '_');
        var handlerWrapperNamespace = $"global::{analyzer.MediatorNamespace}.Internals";
        HandlerWrapperTypeNameWithGenericTypeArguments =
            $"{handlerWrapperNamespace}.NotificationHandlerWrapper<{FullName}>";
        HandlerWrapperPropertyName = $"Wrapper_For_{identifierFullName}";
    }

    public string HandlerWrapperTypeNameWithGenericTypeArguments { get; }

    public string HandlerWrapperPropertyName { get; }
}
