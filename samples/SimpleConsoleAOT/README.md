## SimpleConsoleAOT

Just like SimpleConsole, but with [NativeAOT](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT) enabled.

### Build and run

```sh
$ dotnet publish -r linux-amd64 -c Release
$ ./bin/Release/net8.0/linux-x64/publish/SimpleConsoleAOT
1) Running logger handler
2) Running ping validator
3) Valid input!
4) Returning pong!
5) No error!
-----------------------------------
ID: a6c58809-64ab-4c51-9801-f55cbacca3fe
Ping { Id = a6c58809-64ab-4c51-9801-f55cbacca3fe }
Pong { Id = a6c58809-64ab-4c51-9801-f55cbacca3fe }
```

### Comparison

Below the SimpleConsoleAOT project is benchmarked against SimpleConsole using [hyperfine](https://github.com/sharkdp/hyperfine).

```sh
$ hyperfine './SimpleConsole/bin/Release/net8.0/linux-x64/publish/SimpleConsole' './SimpleCo
nsoleAOT/bin/Release/net8.0/linux-x64/publish/SimpleConsoleAOT'
Benchmark 1: ./SimpleConsole/bin/Release/net8.0/linux-x64/publish/SimpleConsole
  Time (mean ± σ):     100.0 ms ±  17.0 ms    [User: 32.9 ms, System: 9.1 ms]
  Range (min … max):    71.6 ms … 136.4 ms    34 runs

Benchmark 2: ./SimpleConsoleAOT/bin/Release/net8.0/linux-x64/publish/SimpleConsoleAOT
  Time (mean ± σ):       2.7 ms ±   0.4 ms    [User: 2.5 ms, System: 0.3 ms]
  Range (min … max):     2.1 ms …   6.1 ms    941 runs

  Warning: Command took less than 5 ms to complete. Note that the results might be inaccurate because hyperfine can not calibrate the shell startup time much more precise than this limit. You can try to use the `-N`/`--shell=none` option to disable the shell completely.
  Warning: Statistical outliers were detected. Consider re-running this benchmark on a quiet system without any interferences from other programs. It might help to use the '--warmup' or '--prepare' options.

Summary
  ./SimpleConsoleAOT/bin/Release/net8.0/linux-x64/publish/SimpleConsoleAOT ran
   37.34 ± 8.09 times faster than ./SimpleConsole/bin/Release/net8.0/linux-x64/publish/SimpleConsole
```
