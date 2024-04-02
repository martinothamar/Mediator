global using static Mediator.Tests.Assertions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

#pragma warning disable CS0162 // Unreachable code detected

internal static class Assertions
{
    public static void AssertInstanceIdCount(int expected, ConcurrentDictionary<Guid, int> instanceIds, Guid id)
    {
        if (Mediator.ServiceLifetime == ServiceLifetime.Transient)
            return;
        Assert.Equal(expected, instanceIds.GetValueOrDefault(id, 0));
    }

    public static void AssertInstanceIdCount(
        int expected,
        ConcurrentDictionary<Guid, (int Count, long Timestamp)> instanceIds,
        Guid id,
        long timestampBefore,
        long timestampAfter
    )
    {
        if (Mediator.ServiceLifetime == ServiceLifetime.Transient)
            return;
        var data = instanceIds.GetValueOrDefault(id, default);
        Assert.Equal(expected, data.Count);
        Assert.True(data.Timestamp > timestampBefore && data.Timestamp < timestampAfter);
    }
}

#pragma warning restore CS0162 // Unreachable code detected
