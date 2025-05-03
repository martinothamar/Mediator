using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed record NotificationMessageHandlerModel : SymbolMetadataModel
{
    public NotificationMessageHandlerModel(NotificationMessageHandler handler, CompilationAnalyzer analyzer)
        : base(handler.Symbol)
    {
        ServiceRegistrations = ImmutableEquatableArray<string>.Empty;
        if (handler.Messages.Count > 0)
        {
            var interfaceSymbol = handler.InterfaceSymbol.GetTypeSymbolFullName(includeTypeParameters: false);
            var concreteSymbol = handler.Symbol.GetTypeSymbolFullName(includeTypeParameters: false);
            var builder = new List<string>(handler.Messages.Count);

            if (!handler.Symbol.IsGenericType)
            {
                var concreteRegistration =
                    $"services.TryAdd(new SD(typeof({concreteSymbol}), typeof({concreteSymbol}), {analyzer.ServiceLifetime}));";
                builder.Add(concreteRegistration);
            }

            foreach (var message in handler.Messages)
            {
                var requestType = message.Symbol.GetTypeSymbolFullName();
                if (handler.Symbol.IsGenericType)
                {
                    var concreteRegistration =
                        $"services.TryAdd(new SD(typeof({concreteSymbol}<{requestType}>), typeof({concreteSymbol}<{requestType}>), {analyzer.ServiceLifetime}));";
                    builder.Add(concreteRegistration);
                }
                var getExpression =
                    $"GetRequiredService<{concreteSymbol}{(handler.Symbol.IsGenericType ? $"<{requestType}>" : "")}>()";
                var registration =
                    $"services.Add(new SD(typeof({interfaceSymbol}<{requestType}>), {getExpression}, {analyzer.ServiceLifetime}));";
                builder.Add(registration);
            }

            ServiceRegistrations = builder.ToImmutableEquatableArray();
        }
    }

    public ImmutableEquatableArray<string> ServiceRegistrations { get; }
}
