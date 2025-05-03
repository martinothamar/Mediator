using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed class NotificationMessageHandler : MessageHandler<NotificationMessageHandler>
{
    public readonly INamedTypeSymbol InterfaceSymbol;
    public readonly INamedTypeSymbol UnconstructedInterfaceSymbol;
    private readonly List<NotificationMessage> _messages;

    public IReadOnlyList<NotificationMessage> Messages => _messages;

    public NotificationMessageHandler(
        INamedTypeSymbol symbol,
        INamedTypeSymbol interfaceSymbol,
        CompilationAnalyzer analyzer
    )
        : base(symbol, analyzer)
    {
        UnconstructedInterfaceSymbol = interfaceSymbol;
        InterfaceSymbol = symbol.AllInterfaces.Single(i =>
            i.IsGenericType && i.OriginalDefinition.Equals(UnconstructedInterfaceSymbol, SymbolEqualityComparer.Default)
        );
        _messages = new List<NotificationMessage>();
    }

    public bool TryAddMessage(NotificationMessage message)
    {
        if (Symbol.IsGenericType && Symbol.TypeParameters.Length == 1)
        {
            var messageSymbol = message.Symbol;

            var compilation = Analyzer.Compilation;
            if (Symbol.SatisfiesConstraints([messageSymbol], compilation))
            {
                _messages.Add(message);
                return true;
            }
        }
        else
        {
            var serviceType = UnconstructedInterfaceSymbol.Construct([message.Symbol]);
            if (Analyzer.Compilation.HasImplicitConversion(Symbol, serviceType))
            {
                _messages.Add(message);
                return true;
            }
        }

        return false;
    }

    public NotificationMessageHandlerModel ToModel()
    {
        return new NotificationMessageHandlerModel(this, Analyzer);
    }
}
