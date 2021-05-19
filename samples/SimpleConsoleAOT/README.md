## SimpleConsoleAOT

Just like SimpleConsole, but with [NativeAOT](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT) enabled.

### Build and run

```pwsh
PS C:\code\Mediator\samples\SimpleConsoleAOT> dotnet publish -r win-x64 -c release -o dist
PS C:\code\Mediator\samples\SimpleConsoleAOT> .\dist\SimpleConsoleAOT.exe
1) Running logger handler
2) Running ping validator
3) Valid input!
4) Returning pong!
5) No error!
-----------------------------------
ID: 03a0053a-5411-43d6-9f79-c8c293b5e5fe
Ping { Id = 03a0053a-5411-43d6-9f79-c8c293b5e5fe }
Pong { Id = 03a0053a-5411-43d6-9f79-c8c293b5e5fe }
```

### Comparison

This comparison was done locally on my machine, and was pretty consistent.
What we expect from AOT is faster startup time.

```pwsh
PS C:\code\Mediator\samples\SimpleConsole> Measure-Command { .\dist\SimpleConsole.exe }


Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 0
Milliseconds      : 127
Ticks             : 1271741
TotalDays         : 1,4719224537037E-06
TotalHours        : 3,53261388888889E-05
TotalMinutes      : 0,00211956833333333
TotalSeconds      : 0,1271741
TotalMilliseconds : 127,1741

PS C:\code\Mediator\samples\SimpleConsoleAOT> Measure-Command { .\dist\SimpleConsoleAOT.exe }


Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 0
Milliseconds      : 20
Ticks             : 200063
TotalDays         : 2,31554398148148E-07
TotalHours        : 5,55730555555556E-06
TotalMinutes      : 0,000333438333333333
TotalSeconds      : 0,0200063
TotalMilliseconds : 20,0063
```
