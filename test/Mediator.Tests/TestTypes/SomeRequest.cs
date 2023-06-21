using System;

namespace Mediator.Tests.TestTypes;

public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>;

public sealed record SomeRequestWithoutResponse(Guid Id) : IRequest;

public sealed record SomeRequestReturningByteArray(Guid Id) : IRequest<byte[]>;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public sealed record SomeRequestWithoutHandler(Guid Id) : IRequest;
#pragma warning restore MSG0005 // MediatorGenerator message warning
