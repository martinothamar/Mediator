using Mediator;

namespace AspNetCore.ModularArchitecture.ModuleB;

public sealed record GetModuleBPublicGreeting(string Name) : IRequest<string>;

public sealed class GetModuleBPublicGreetingHandler : IRequestHandler<GetModuleBPublicGreeting, string>
{
    public ValueTask<string> Handle(GetModuleBPublicGreeting request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"ModuleB public handler says hello to {request.Name}.");
    }
}
