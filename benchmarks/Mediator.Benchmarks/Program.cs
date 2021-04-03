using BenchmarkDotNet.Running;
using Mediator.Benchmarks.Request;

namespace Mediator.Benchmarks
{
    class Program
    {
        static void Main() => BenchmarkRunner.Run<RequestBenchmarks>();
    }
}
