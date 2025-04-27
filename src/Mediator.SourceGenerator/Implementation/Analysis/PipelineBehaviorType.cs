namespace Mediator.SourceGenerator;

internal sealed class PipelineBehaviorType : SymbolMetadata<PipelineBehaviorType>
{
    public readonly INamedTypeSymbol InterfaceSymbol;
    private readonly List<RequestMessage> _messages;

    public IReadOnlyList<RequestMessage> Messages => _messages;

    public PipelineBehaviorType(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
        : base(symbol, analyzer)
    {
        InterfaceSymbol = symbol.AllInterfaces.Single(i =>
            (i.Name == "IPipelineBehavior" || i.Name == "IStreamPipelineBehavior")
            && i.IsGenericType
            && i.TypeArguments.Length == 2
        );
        _messages = new List<RequestMessage>();
    }

    public void TryAddMessage(RequestMessage message)
    {
        if (Symbol.IsGenericType && Symbol.TypeParameters.Length == 2)
        {
            var messageSymbol = message.Symbol;
            var constraints = Symbol.TypeParameters[0].ConstraintTypes;
            var compilation = Analyzer.Compilation;
            if (constraints.All(constraint => compilation.HasImplicitConversion(messageSymbol, constraint)))
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
