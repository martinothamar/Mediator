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
            var sd = "global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor";
            var interfaceSymbol = handler.UnconstructedInterfaceSymbol.GetTypeSymbolFullName(
                includeTypeParameters: false
            );
            var concreteSymbol = handler.Symbol.GetTypeSymbolFullName(includeTypeParameters: false);
            var builder = new List<string>(handler.Messages.Count);

            if (!handler.Symbol.IsGenericType)
            {
                var concreteRegistration = $"""
                    if (!IsHandlerAlreadyRegistered(existingRegistrations, typeof({concreteSymbol}), typeof({concreteSymbol})))
                       services.TryAdd(new {sd}(typeof({concreteSymbol}), typeof({concreteSymbol}), {analyzer.ServiceLifetime}));
                    """;
                builder.Add(concreteRegistration);
            }

            foreach (var message in handler.Messages)
            {
                var requestType = message.Symbol.GetTypeSymbolFullName();
                if (handler.Symbol.IsGenericType)
                {
                    var concreteRegistration = $"""
                        if (!IsHandlerAlreadyRegistered(existingRegistrations, typeof({concreteSymbol}<{requestType}>), typeof({concreteSymbol}<{requestType}>)))
                           services.TryAdd(new {sd}(typeof({concreteSymbol}<{requestType}>), typeof({concreteSymbol}<{requestType}>), {analyzer.ServiceLifetime}));
                        """;
                    builder.Add(concreteRegistration);
                }

                var concreteImpl = $"{concreteSymbol}{(handler.Symbol.IsGenericType ? $"<{requestType}>" : "")}";
                var getExpression = $"GetRequiredService<{concreteImpl}>()";
                var registration = $"""
                    if (!IsHandlerAlreadyRegistered(existingRegistrations, typeof({interfaceSymbol}<{requestType}>), typeof({concreteImpl})))
                       services.Add(new {sd}(typeof({interfaceSymbol}<{requestType}>), {getExpression}, {analyzer.ServiceLifetime}));
                    """;
                builder.Add(registration);
            }

            ServiceRegistrations = builder.ToImmutableEquatableArray();
        }
    }

    public ImmutableEquatableArray<string> ServiceRegistrations { get; }
}
