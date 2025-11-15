using System;
using System.Threading.Tasks;
using JetBrains.dotMemoryUnit;
using JetBrains.dotMemoryUnit.Kernel;
using Microsoft.Extensions.DependencyInjection;

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

        await using var sp = services.BuildServiceProvider(
            new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
        );
        var mediator = sp.GetRequiredService<Mediator>();

        var id = Guid.NewGuid();
        var request = new SomeRequestMemAllocTracking(id);
        var cancellationToken = TestContext.Current.CancellationToken;

        // Ensure everything is cached
        await mediator.Send(request, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.WaitForFullGCComplete();
        GC.Collect();

        var checkpoint = dotMemory.Check();
        var beforeBytes = GC.GetAllocatedBytesForCurrentThread();
        await mediator.Send(request, cancellationToken);
        var afterBytes = GC.GetAllocatedBytesForCurrentThread();

        Assert.Equal(0, afterBytes - beforeBytes);
        dotMemory.Check(memory =>
        {
            var traffic = memory.GetDifference(checkpoint);
            var newObjects = traffic.GetNewObjects();
            foreach (var obj in newObjects.GroupByType())
                _output.WriteLine($"Allocations for {obj.TypeFullyQualifiedName}: {obj.SizeInBytes} bytes");

            //_output.WriteLine($"Allocated: {traffic.AllocatedMemory.SizeInBytes}");
            //foreach (var obj in traffic.GroupByType())
            //    _output.WriteLine($"Allocations for {obj.TypeFullyQualifiedName}: {obj.AllocatedMemoryInfo.SizeInBytes}");

            Assert.Equal(0, traffic.GetNewObjects().ObjectsCount);
        });
    }

    [Fact]
    public void Test_Request_Handler()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        using var sp = services.BuildServiceProvider(
            new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
        );
        var mediator = sp.GetRequiredService<IMediator>();

        var id = Guid.NewGuid();
        var request = new SomeRequestMemAllocTracking(id);
        var cancellationToken = TestContext.Current.CancellationToken;

        // We suppress here since we know that this isn't blocking
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        // Ensure everything is cached
        mediator.Send(request, cancellationToken).GetAwaiter().GetResult();

        var beforeBytes = Allocations.GetCurrentThreadAllocatedBytes();
        mediator.Send(request, cancellationToken).GetAwaiter().GetResult();
        var afterBytes = Allocations.GetCurrentThreadAllocatedBytes();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        Assert.Equal(0, afterBytes - beforeBytes);
    }
}
