## Benchmarks

Here are some of the benchmarks I've run on my slightly old development laptop.

### Requests

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.300-preview.21228.15
  [Host]     : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT
  DefaultJob : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT


```
|                        Method |       Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------ |-----------:|----------:|----------:|------:|-------:|------:|------:|----------:|
|          SendRequest_Baseline |   9.619 ns | 0.0163 ns | 0.0144 ns | 0.010 |      - |     - |     - |         - |
| SendRequest_Mediator_Concrete |  11.782 ns | 0.0439 ns | 0.0411 ns | 0.012 |      - |     - |     - |         - |
|       SendRequest_MessagePipe |  14.277 ns | 0.0334 ns | 0.0312 ns | 0.014 |      - |     - |     - |         - |
|          SendRequest_Mediator |  27.782 ns | 0.3022 ns | 0.2678 ns | 0.028 |      - |     - |     - |         - |
|           SendRequest_MediatR | 993.866 ns | 2.9908 ns | 2.7976 ns | 1.000 | 0.4349 |     - |     - |    1368 B |

### Big struct requests

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.300-preview.21228.15
  [Host]     : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT
  DefaultJob : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT


```
|                              Method |         Mean |      Error |     StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------ |-------------:|-----------:|-----------:|------:|-------:|------:|------:|----------:|
|          SendStructRequest_Baseline |     9.372 ns |  0.0061 ns |  0.0054 ns | 0.008 |      - |     - |     - |         - |
| SendStructRequest_Mediator_Concrete |    16.817 ns |  0.0609 ns |  0.0509 ns | 0.014 |      - |     - |     - |         - |
|       SendStructRequest_MessagePipe |    25.740 ns |  0.0838 ns |  0.0784 ns | 0.021 |      - |     - |     - |         - |
|          SendStructRequest_Mediator |    55.419 ns |  1.1665 ns |  1.9165 ns | 0.046 | 0.0280 |     - |     - |      88 B |
|           SendStructRequest_MediatR | 1,211.924 ns | 24.0487 ns | 40.8366 ns | 1.000 | 0.4635 |     - |     - |    1456 B |
