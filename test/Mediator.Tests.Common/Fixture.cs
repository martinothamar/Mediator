using System;

namespace Mediator.Tests.Common;

public static class Allocations
{
    public static long GetCurrentThreadAllocatedBytes()
    {
        // Run before and after message sending to test allocations.
        // Stolen from https://github.com/dotnet/BenchmarkDotNet/blob/4bd433d85fff4fb6ba8c4f8df3e685ad669e2519/src/BenchmarkDotNet/Engines/GcStats.cs#L132
        // There is a reason GC.Collect is run first
        GC.Collect();
        return GC.GetAllocatedBytesForCurrentThread();
    }

    public static long GetCurrentAllocatedBytes()
    {
        // Run before and after message sending to test allocations.
        // Stolen from https://github.com/dotnet/BenchmarkDotNet/blob/4bd433d85fff4fb6ba8c4f8df3e685ad669e2519/src/BenchmarkDotNet/Engines/GcStats.cs#L132
        // There is a reason GC.Collect is run first
        GC.Collect();
        return GC.GetTotalAllocatedBytes(true);
    }
}
