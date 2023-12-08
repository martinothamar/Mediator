﻿using Mediator;

namespace InternalMessages.Domain;

internal sealed record Ping(Guid Id) : IRequest<Pong>;

internal sealed record Pong(Guid Id);
