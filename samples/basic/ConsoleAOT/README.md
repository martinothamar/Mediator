## ConsoleAOT

Just like Console, but compiled with [Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=linux-ubuntu%2Cnet8).

### Build and run

Replace `<RID>` with your runtime below, e.g. `win-x64` or `linux-x64`.

```console
$ dotnet publish -r <RID> -c Release
$ ./bin/Release/net10.0/<RID>/ConsoleAOT
1) Running logger handler
2) Running ping validator
3) Valid input!
4) Returning pong!
5) No error!
-----------------------------------
ID: 2dded749-c651-49d3-b7b9-5669ce953c52
Ping { Id = 2dded749-c651-49d3-b7b9-5669ce953c52 }
Pong { Id = 2dded749-c651-49d3-b7b9-5669ce953c52 }
```

### Comparison

Below the ConsoleAOT project is benchmarked against Console using [hyperfine](https://github.com/sharkdp/hyperfine).

```console
$ hyperfine './Console/bin/Release/net10.0/linux-x64/publish/Console' './ConsoleAOT/bin/Release/net10.0/linux-x64/publish/ConsoleAOT'
Benchmark 1: ./Console/bin/Release/net10.0/linux-x64/publish/Console
  Time (mean ± σ):     100.0 ms ±  17.0 ms    [User: 32.9 ms, System: 9.1 ms]
  Range (min … max):    71.6 ms … 136.4 ms    34 runs

Benchmark 2: ./ConsoleAOT/bin/Release/net10.0/linux-x64/publish/ConsoleAOT
  Time (mean ± σ):       2.7 ms ±   0.4 ms    [User: 2.5 ms, System: 0.3 ms]
  Range (min … max):     2.1 ms …   6.1 ms    941 runs

  Warning: Command took less than 5 ms to complete. Note that the results might be inaccurate because hyperfine can not calibrate the shell startup time much more precise than this limit. You can try to use the `-N`/`--shell=none` option to disable the shell completely.
  Warning: Statistical outliers were detected. Consider re-running this benchmark on a quiet system without any interferences from other programs. It might help to use the '--warmup' or '--prepare' options.

Summary
  ./ConsoleAOT/bin/Release/net10.0/linux-x64/publish/ConsoleAOT ran
   37.34 ± 8.09 times faster than ./Console/bin/Release/net10.0/linux-x64/publish/Console
```
