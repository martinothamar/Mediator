using Mediator;

namespace AspNetCore.ModularArchitecture.ModuleA;

public sealed record GetModuleAGreeting(string Name) : IRequest<string>;

public sealed class GetModuleAGreetingHandler : IRequestHandler<GetModuleAGreeting, string>
{
    public ValueTask<string> Handle(GetModuleAGreeting request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"ModuleA says hello to {request.Name}.");
    }
}
