using Mediator.SourceGenerator.Extensions;

namespace Mediator.SourceGenerator;

internal abstract class MessageHandler<T> : SymbolMetadata<MessageHandler<T>>
{
    protected MessageHandler(INamedTypeSymbol symbol, CompilationAnalyzer analyzer)
        : base(symbol, analyzer) { }

    public string FullName => Symbol.GetTypeSymbolFullName();
}
