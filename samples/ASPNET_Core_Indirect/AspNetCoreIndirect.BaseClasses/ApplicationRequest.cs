using Mediator;

namespace AspNetCoreIndirect.BaseClasses;

public abstract record ApplicationRequest<TResponse> : IRequest<TResponse>;
