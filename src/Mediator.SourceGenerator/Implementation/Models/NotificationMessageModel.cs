using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record NotificationMessageModel : SymbolMetadataModel
{
    public NotificationMessageModel(
        INamedTypeSymbol symbol,
        CompilationAnalyzer analyzer,
        HashSet<NotificationMessageHandler> handlers
    )
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
        var handlerServicesRegistrationBlock = new string[handlers.Count];
        var handlerTypeOfExpression = $"typeof(global::Mediator.INotificationHandler<{FullName}>)";
        int i = 0;
        foreach (var handler in handlers)
        {
            var getExpression = $"GetRequiredService<{handler.FullName}>()";
            handlerServicesRegistrationBlock[i] =
                $"services.Add(new SD({handlerTypeOfExpression}, {getExpression}, {analyzer.ServiceLifetime}));";
            i++;
        }
        HandlerServicesRegistrationBlock = new(handlerServicesRegistrationBlock);
    }

    public string HandlerWrapperTypeNameWithGenericTypeArguments { get; }

    public string HandlerWrapperPropertyName { get; }

    public ImmutableEquatableArray<string> HandlerServicesRegistrationBlock { get; }
}
