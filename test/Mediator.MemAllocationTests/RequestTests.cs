using JetBrains.dotMemoryUnit;
using JetBrains.dotMemoryUnit.Kernel;
using Mediator.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Mediator.MemAllocationTests;

[Collection("Non-Parallel")]
public class RequestTests
{
    private readonly ITestOutputHelper _output;

    public RequestTests(ITestOutputHelper output)
    {
        _output = output;
        DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
    }

    /// <summary>
    /// To run these tests
    /// dotMemoryUnit.exe "$((Get-Command dotnet.exe).Path)" -- test "./test/Mediator.MemAllocationTests/"
    /// TODO: find a way to filter out dotMemoryUnit's own mem allocations from report...
    /// </summary>
    /// <returns></returns>
    [DotMemoryUnit(CollectAllocations = true)]
    [Fact]
    public async Task Test_Request_Handler_dotMemory()
    {
        if (!dotMemoryApi.IsEnabled)
            return;

        var services = new ServiceCollection();

        services.AddMediator();

        await using var sp = services.BuildServiceProvider(validateScopes: true);
        var mediator = sp.GetRequiredService<Mediator>();

        var id = Guid.NewGuid();
        var request = new SomeRequestMemAllocTracking(id);

        // Ensure everything is cached
        await mediator.Send(request);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.WaitForFullGCComplete();
        GC.Collect();

        var checkpoint = dotMemory.Check();
        var beforeBytes = GC.GetAllocatedBytesForCurrentThread();
        await mediator.Send(request);
        var afterBytes = GC.GetAllocatedBytesForCurrentThread();

        Assert.Equal(0, afterBytes - beforeBytes);
        dotMemory.Check(
            memory =>
            {
                var traffic = memory.GetDifference(checkpoint);
                var newObjects = traffic.GetNewObjects();
                foreach (var obj in newObjects.GroupByType())
                    _output.WriteLine($"Allocations for {obj.TypeFullyQualifiedName}: {obj.SizeInBytes} bytes");

                //_output.WriteLine($"Allocated: {traffic.AllocatedMemory.SizeInBytes}");
                //foreach (var obj in traffic.GroupByType())
                //    _output.WriteLine($"Allocations for {obj.TypeFullyQualifiedName}: {obj.AllocatedMemoryInfo.SizeInBytes}");

                Assert.Equal(0, traffic.GetNewObjects().ObjectsCount);
            }
        );
    }

    [Fact]
    public void Test_Request_Handler()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        using var sp = services.BuildServiceProvider(validateScopes: true);
        var mediator = sp.GetRequiredService<IMediator>();

        var id = Guid.NewGuid();
        var request = new SomeRequestMemAllocTracking(id);

        // Ensure everything is cached
        mediator.Send(request); // Everything returns sync

        var beforeBytes = Allocations.GetCurrentThreadAllocatedBytes();
        mediator.Send(request); // Everything returns sync
        var afterBytes = Allocations.GetCurrentThreadAllocatedBytes();

        Assert.Equal(0, afterBytes - beforeBytes);
    }
}
