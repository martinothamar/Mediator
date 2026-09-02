using Mediator;

namespace AspNetCore.ModularArchitecture.ModuleB;

public sealed record GetModuleBInternalGreeting(string Name) : IRequest<string>;

internal sealed class GetModuleBInternalGreetingHandler : IRequestHandler<GetModuleBInternalGreeting, string>
{
    public ValueTask<string> Handle(GetModuleBInternalGreeting request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"ModuleB internal handler says hello to {request.Name}.");
    }
}
