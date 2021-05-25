## Benchmarks

Here are some of the benchmarks I've run on my slightly old development laptop.

### Requests

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.100-preview.3.21202.5
  [Host]     : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT
  DefaultJob : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT


```
|                        Method |         Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------ |-------------:|----------:|----------:|------:|-------:|------:|------:|----------:|
|          SendRequest_Baseline |     9.549 ns | 0.0494 ns | 0.0438 ns | 0.009 |      - |     - |     - |         - |
| SendRequest_Mediator_Concrete |    11.762 ns | 0.0817 ns | 0.0724 ns | 0.011 |      - |     - |     - |         - |
|       SendRequest_MessagePipe |    14.242 ns | 0.0580 ns | 0.0453 ns | 0.013 |      - |     - |     - |         - |
|          SendRequest_Mediator |    29.810 ns | 0.1220 ns | 0.0953 ns | 0.027 |      - |     - |     - |         - |
|           SendRequest_MediatR | 1,085.840 ns | 8.0984 ns | 6.3227 ns | 1.000 | 0.4349 |     - |     - |   1,368 B |


### Big struct requests

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.100-preview.3.21202.5
  [Host]     : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT
  DefaultJob : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT


```
|                              Method |         Mean |      Error |     StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------ |-------------:|-----------:|-----------:|------:|-------:|------:|------:|----------:|
|          SendStructRequest_Baseline |     9.866 ns |  0.3014 ns |  0.3918 ns | 0.009 |      - |     - |     - |         - |
| SendStructRequest_Mediator_Concrete |    16.580 ns |  0.0835 ns |  0.0781 ns | 0.015 |      - |     - |     - |         - |
|       SendStructRequest_MessagePipe |    25.852 ns |  0.2399 ns |  0.2244 ns | 0.023 |      - |     - |     - |         - |
|          SendStructRequest_Mediator |    50.425 ns |  0.3834 ns |  0.3202 ns | 0.044 | 0.0280 |     - |     - |      88 B |
|           SendStructRequest_MediatR | 1,133.261 ns | 19.6696 ns | 18.3989 ns | 1.000 | 0.4635 |     - |     - |   1,456 B |

