using System;

namespace Mediator.Tests.TestTypes;

public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>;

public sealed record SomeRequestWithoutResponse(Guid Id) : IRequest;

public sealed record SomeRequestReturningByteArray(Guid Id) : IRequest<byte[]>;
