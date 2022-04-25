namespace Mediator.SourceGenerator;

internal sealed class SyntaxReceiver : ISyntaxReceiver
{
    public List<InvocationExpressionSyntax> AddMediatorCalls { get; } = new List<InvocationExpressionSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode context)
    {
        if (ShouldVisit(context, out var invocationSyntax))
            AddMediatorCalls.Add(invocationSyntax!);
    }

    public static bool ShouldVisit(SyntaxNode context, out InvocationExpressionSyntax? invocation)
    {
        invocation = null;
        if (
            context
            is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax identifier } invocationSyntax
        )
            return false;
        if (identifier.Name.Identifier.ValueText != "AddMediator")
            return false;

        invocation = invocationSyntax;
        return true;
    }
}
