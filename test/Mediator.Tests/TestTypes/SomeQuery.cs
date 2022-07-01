using System;

namespace Mediator.Tests.TestTypes;

public sealed record SomeQuery(Guid Id) : IQuery<SomeResponse>;
