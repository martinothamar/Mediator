using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal abstract class MessageHandler<T> : SymbolMetadata<MessageHandler<T>>
{
    protected MessageHandler(INamedTypeSymbol symbol, CompilationAnalyzer analyzer) : base(symbol, analyzer) { }

    public bool IsOpenGeneric => Symbol.TypeArguments.Length > 0;

    public string FullName => Symbol.GetTypeSymbolFullName();
}
