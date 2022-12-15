using System;

namespace Mediator.Tests.TestTypes;

public sealed record SomeQuery(Guid Id) : IQuery<SomeResponse>;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public sealed record SomeQueryWithoutHandler(Guid Id) : IQuery<SomeResponse>;
#pragma warning restore MSG0005 // MediatorGenerator message warning
