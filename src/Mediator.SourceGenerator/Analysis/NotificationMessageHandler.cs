namespace Mediator.SourceGenerator;

internal sealed class NotificationMessageHandler : MessageHandler<NotificationMessageHandler>
{
    public NotificationMessageHandler(INamedTypeSymbol symbol, CompilationAnalyzer analyzer) : base(symbol, analyzer)
    { }

    public string OpenGenericTypeOfExpression => $"typeof(global::Mediator.INotificationHandler<>)";

    public string OpenGenericServiceRegistrationBlock =>
        $"services.Add(new SD({OpenGenericTypeOfExpression}, {TypeOfExpression(false)}, {ServiceLifetime}));";
}
