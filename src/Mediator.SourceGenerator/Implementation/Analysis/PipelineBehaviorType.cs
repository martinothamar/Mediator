using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal sealed class PipelineBehaviorType : SymbolMetadata<PipelineBehaviorType>
{
    public readonly INamedTypeSymbol InterfaceSymbol;
    private readonly List<RequestMessage> _messages;

    public IReadOnlyList<RequestMessage> Messages => _messages;

    public PipelineBehaviorType(INamedTypeSymbol symbol, INamedTypeSymbol interfaceSymbol, CompilationAnalyzer analyzer)
        : base(symbol, analyzer)
    {
        InterfaceSymbol = symbol.AllInterfaces.Single(i =>
            i.ConstructUnboundGenericType()
                .Equals(interfaceSymbol.ConstructUnboundGenericType(), SymbolEqualityComparer.Default)
        );
        _messages = new List<RequestMessage>();
    }

    public void TryAddMessage(RequestMessage message)
    {
        if (Symbol.IsGenericType && Symbol.TypeParameters.Length == 2)
        {
            var messageSymbol = message.Symbol;
            var responseSymbol = message.ResponseSymbol;

            var compilation = Analyzer.Compilation;
            if (Symbol.SatisfiesConstraints([messageSymbol, responseSymbol], compilation))
            {
                _messages.Add(message);
            }
        }
        else
        {
            var requestType = InterfaceSymbol.TypeArguments[0];
            if (message.Symbol.Equals(requestType, SymbolEqualityComparer.Default))
            {
                _messages.Add(message);
            }
        }
    }

    public PipelineBehaviorModel ToModel()
    {
        return new PipelineBehaviorModel(this, Analyzer);
    }
}
