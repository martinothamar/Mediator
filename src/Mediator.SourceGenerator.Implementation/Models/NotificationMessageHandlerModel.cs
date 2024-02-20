namespace Mediator.SourceGenerator;

internal sealed record NotificationMessageHandlerModel : MessageHandlerModel
{
    const string OpenGenericTypeOfExpression = $"typeof(global::Mediator.INotificationHandler<>)";

    public NotificationMessageHandlerModel(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
        : base(symbol, analyzer)
    {
        OpenGenericServiceRegistrationBlock =
            $"services.Add(new SD({OpenGenericTypeOfExpression}, {TypeOfExpression(symbol, false)}, {ServiceLifetime}));";
    }

    public string OpenGenericServiceRegistrationBlock { get; }
}
