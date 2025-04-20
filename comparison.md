## Current:

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.408
  [Host]          : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Scoped/False    : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Scoped/True     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Singleton/False : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Singleton/True  : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Transient/False : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Transient/True  : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


```
| Method                         | Categories             | ServiceLifetime | Project type | Mean       | Error     | StdDev    | Median     | Ratio         | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------------------- |---------------- |------------- |-----------:|----------:|----------:|-----------:|--------------:|--------:|-----:|-------:|----------:|------------:|
| ColdStart_MediatR              | ColdStart              | Scoped          | Small        | 198.380 ns | 3.7555 ns | 3.5129 ns | 200.570 ns |      baseline |         |    1 | 0.0348 |     584 B |             |
| ColdStart_IMediator            | ColdStart              | Scoped          | Small        | 348.325 ns | 3.4330 ns | 3.2112 ns | 346.886 ns |  1.76x slower |   0.03x |    2 | 0.0520 |     872 B |  1.49x more |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| ColdStart_MediatR              | ColdStart              | Scoped          | Large        | 210.619 ns | 2.0817 ns | 1.9472 ns | 211.183 ns |      baseline |         |    1 | 0.0348 |     584 B |             |
| ColdStart_IMediator            | ColdStart              | Scoped          | Large        | 360.949 ns | 7.2398 ns | 7.1104 ns | 357.257 ns |  1.71x slower |   0.04x |    2 | 0.0520 |     872 B |  1.49x more |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| ColdStart_IMediator            | ColdStart              | Singleton       | Small        |  27.538 ns | 0.4142 ns | 0.3875 ns |  27.585 ns |  3.65x faster |   0.06x |    1 |      - |         - |          NA |
| ColdStart_MediatR              | ColdStart              | Singleton       | Small        | 100.620 ns | 1.0707 ns | 1.0016 ns | 101.058 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| ColdStart_MediatR              | ColdStart              | Singleton       | Large        | 108.935 ns | 0.9321 ns | 0.8719 ns | 109.159 ns |      baseline |         |    1 | 0.0143 |     240 B |             |
| ColdStart_IMediator            | ColdStart              | Singleton       | Large        | 168.109 ns | 2.1708 ns | 2.0306 ns | 167.542 ns |  1.54x slower |   0.02x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| ColdStart_MediatR              | ColdStart              | Transient       | Small        | 108.394 ns | 1.4002 ns | 1.2412 ns | 108.003 ns |      baseline |         |    1 | 0.0162 |     272 B |             |
| ColdStart_IMediator            | ColdStart              | Transient       | Small        | 131.020 ns | 1.8479 ns | 1.7285 ns | 130.050 ns |  1.21x slower |   0.02x |    2 | 0.0119 |     200 B |  1.36x less |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| ColdStart_MediatR              | ColdStart              | Transient       | Large        | 125.671 ns | 0.3392 ns | 0.3172 ns | 125.760 ns |      baseline |         |    1 | 0.0162 |     272 B |             |
| ColdStart_IMediator            | ColdStart              | Transient       | Large        | 150.637 ns | 1.6483 ns | 1.5418 ns | 150.424 ns |  1.20x slower |   0.01x |    2 | 0.0119 |     200 B |  1.36x less |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Initialization_MediatR         | Initialization         | Scoped          | Small        |  74.303 ns | 1.1820 ns | 1.1056 ns |  73.740 ns |      baseline |         |    1 | 0.0205 |     344 B |             |
| Initialization_IMediator       | Initialization         | Scoped          | Small        | 126.104 ns | 1.5053 ns | 1.4080 ns | 126.090 ns |  1.70x slower |   0.03x |    2 | 0.0210 |     352 B |  1.02x more |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Initialization_MediatR         | Initialization         | Scoped          | Large        |  76.246 ns | 0.7385 ns | 0.6908 ns |  76.552 ns |      baseline |         |    1 | 0.0205 |     344 B |             |
| Initialization_IMediator       | Initialization         | Scoped          | Large        | 132.576 ns | 2.2281 ns | 1.9752 ns | 132.006 ns |  1.74x slower |   0.03x |    2 | 0.0210 |     352 B |  1.02x more |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Initialization_IMediator       | Initialization         | Singleton       | Small        |   8.719 ns | 0.1235 ns | 0.1155 ns |   8.695 ns |  1.01x faster |   0.02x |    1 |      - |         - |          NA |
| Initialization_MediatR         | Initialization         | Singleton       | Small        |   8.840 ns | 0.0902 ns | 0.0843 ns |   8.832 ns |      baseline |         |    1 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Initialization_IMediator       | Initialization         | Singleton       | Large        |   8.715 ns | 0.1056 ns | 0.0988 ns |   8.757 ns |  1.06x faster |   0.02x |    1 |      - |         - |          NA |
| Initialization_MediatR         | Initialization         | Singleton       | Large        |   9.199 ns | 0.1097 ns | 0.1026 ns |   9.266 ns |      baseline |         |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Initialization_MediatR         | Initialization         | Transient       | Small        |  12.969 ns | 0.1221 ns | 0.1019 ns |  12.930 ns |      baseline |         |    1 | 0.0019 |      32 B |             |
| Initialization_IMediator       | Initialization         | Transient       | Small        |  40.993 ns | 0.5485 ns | 0.5131 ns |  40.997 ns |  3.16x slower |   0.05x |    2 | 0.0024 |      40 B |  1.25x more |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Initialization_MediatR         | Initialization         | Transient       | Large        |  12.948 ns | 0.0506 ns | 0.0422 ns |  12.948 ns |      baseline |         |    1 | 0.0019 |      32 B |             |
| Initialization_IMediator       | Initialization         | Transient       | Large        |  44.880 ns | 0.6910 ns | 0.6464 ns |  44.982 ns |  3.47x slower |   0.05x |    2 | 0.0024 |      40 B |  1.25x more |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Scoped          | Small        |  59.260 ns | 0.4933 ns | 0.4119 ns |  59.086 ns |  1.42x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Small        |  66.459 ns | 0.1539 ns | 0.1285 ns |  66.398 ns |  1.26x faster |   0.02x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Small        |  83.922 ns | 1.2598 ns | 1.1784 ns |  83.993 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Scoped          | Large        |  81.861 ns | 0.9188 ns | 0.8594 ns |  81.253 ns |  1.12x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Large        |  91.548 ns | 1.1106 ns | 0.9845 ns |  91.539 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Large        | 102.573 ns | 0.8898 ns | 0.8323 ns | 102.184 ns |  1.12x slower |   0.01x |    3 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Small        |  20.396 ns | 0.2346 ns | 0.2194 ns |  20.253 ns |  4.12x faster |   0.05x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Small        |  27.252 ns | 0.2938 ns | 0.2748 ns |  27.093 ns |  3.09x faster |   0.04x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Small        |  84.104 ns | 0.7546 ns | 0.7058 ns |  83.818 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Large        |  89.381 ns | 1.3415 ns | 1.2549 ns |  88.500 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Large        | 283.091 ns | 3.6480 ns | 3.4123 ns | 285.335 ns |  3.17x slower |   0.06x |    2 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Large        | 320.386 ns | 3.3276 ns | 3.1126 ns | 318.402 ns |  3.59x slower |   0.06x |    3 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Small        |  58.016 ns | 0.1636 ns | 0.1530 ns |  57.984 ns |  1.60x faster |   0.02x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Small        |  92.756 ns | 1.3326 ns | 1.2465 ns |  92.502 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
| Notification_IMediator         | Notification,Concrete  | Transient       | Small        | 102.012 ns | 1.2801 ns | 1.1974 ns | 101.577 ns |  1.10x slower |   0.02x |    3 | 0.0048 |      80 B |  3.60x less |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Large        |  56.706 ns | 0.7352 ns | 0.6877 ns |  56.683 ns |  1.55x faster |   0.03x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Large        |  87.797 ns | 1.3593 ns | 1.2715 ns |  87.930 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
| Notification_IMediator         | Notification,Concrete  | Transient       | Large        | 103.468 ns | 1.2679 ns | 1.1860 ns | 103.256 ns |  1.18x slower |   0.02x |    3 | 0.0048 |      80 B |  3.60x less |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Small        |  62.088 ns | 0.5397 ns | 0.5048 ns |  62.453 ns |  1.31x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Small        |  81.278 ns | 1.2855 ns | 1.2025 ns |  81.239 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Large        |  82.874 ns | 0.4754 ns | 0.3970 ns |  82.954 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Large        |  93.071 ns | 0.5544 ns | 0.5186 ns |  92.817 ns |  1.12x slower |   0.01x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Small        |  21.882 ns | 0.2236 ns | 0.2091 ns |  21.819 ns |  3.92x faster |   0.06x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Small        |  85.872 ns | 1.1529 ns | 1.0784 ns |  86.018 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Large        |  83.566 ns | 1.0690 ns | 0.9999 ns |  83.529 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Large        | 299.514 ns | 2.9217 ns | 2.7330 ns | 301.064 ns |  3.58x slower |   0.05x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Small        |  64.223 ns | 1.0202 ns | 0.9543 ns |  63.751 ns |  1.57x faster |   0.03x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Small        | 100.585 ns | 1.5443 ns | 1.4445 ns | 100.623 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Large        |  81.933 ns | 1.1616 ns | 1.0866 ns |  82.262 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Large        | 110.026 ns | 1.2951 ns | 1.2114 ns | 110.588 ns |  1.34x slower |   0.02x |    2 | 0.0048 |      80 B |  3.60x less |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Scoped          | Small        |  24.146 ns | 0.3453 ns | 0.3230 ns |  24.145 ns |  3.80x faster |   0.07x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Scoped          | Small        |  35.919 ns | 0.3188 ns | 0.2982 ns |  35.859 ns |  2.55x faster |   0.04x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Scoped          | Small        |  91.697 ns | 1.2087 ns | 1.1306 ns |  91.163 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Scoped          | Large        |  25.404 ns | 0.2253 ns | 0.2107 ns |  25.558 ns |  3.83x faster |   0.05x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Scoped          | Large        |  36.811 ns | 0.3524 ns | 0.3296 ns |  37.027 ns |  2.64x faster |   0.04x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Scoped          | Large        |  97.218 ns | 1.1175 ns | 1.0453 ns |  96.951 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Singleton       | Small        |   8.329 ns | 0.1006 ns | 0.0941 ns |   8.403 ns | 11.95x faster |   0.18x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Singleton       | Small        |  19.101 ns | 0.2558 ns | 0.2393 ns |  19.092 ns |  5.21x faster |   0.08x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Singleton       | Small        |  99.528 ns | 1.1215 ns | 1.0490 ns |  99.261 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_MediatR                | Request,Concrete       | Singleton       | Large        |  98.005 ns | 1.0901 ns | 1.0197 ns |  97.692 ns |      baseline |         |    1 | 0.0143 |     240 B |             |
| Request_IMediator              | Request,Concrete       | Singleton       | Large        | 138.790 ns | 2.0807 ns | 1.9463 ns | 137.290 ns |  1.42x slower |   0.02x |    2 |      - |         - |          NA |
| Request_Mediator               | Request,Concrete       | Singleton       | Large        | 144.181 ns | 1.3824 ns | 1.2931 ns | 145.202 ns |  1.47x slower |   0.02x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Transient       | Small        |  78.551 ns | 1.6110 ns | 1.5069 ns |  77.856 ns |  1.11x faster |   0.02x |    1 | 0.0095 |     160 B |  1.50x less |
| Request_IMediator              | Request,Concrete       | Transient       | Small        |  85.996 ns | 0.1694 ns | 0.1415 ns |  85.973 ns |  1.01x faster |   0.01x |    2 | 0.0095 |     160 B |  1.50x less |
| Request_MediatR                | Request,Concrete       | Transient       | Small        |  86.959 ns | 1.0597 ns | 0.9912 ns |  87.185 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Transient       | Large        |  80.175 ns | 0.8901 ns | 0.8326 ns |  79.846 ns |  1.14x faster |   0.02x |    1 | 0.0095 |     160 B |  1.50x less |
| Request_MediatR                | Request,Concrete       | Transient       | Large        |  91.738 ns | 0.9150 ns | 0.8559 ns |  91.511 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
| Request_IMediator              | Request,Concrete       | Transient       | Large        | 100.202 ns | 1.3541 ns | 1.2666 ns | 100.163 ns |  1.09x slower |   0.02x |    3 | 0.0095 |     160 B |  1.50x less |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Small        |  47.656 ns | 0.3254 ns | 0.2541 ns |  47.474 ns |  2.20x faster |   0.01x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Scoped          | Small        | 104.996 ns | 0.5538 ns | 0.4324 ns | 105.111 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_MediatR_Object         | Request,Object         | Scoped          | Large        | 116.334 ns | 0.1055 ns | 0.0936 ns | 116.337 ns |      baseline |         |    1 | 0.0186 |     312 B |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Large        | 211.924 ns | 2.2916 ns | 2.1436 ns | 213.542 ns |  1.82x slower |   0.02x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Small        |  25.867 ns | 0.3414 ns | 0.3193 ns |  25.639 ns |  4.24x faster |   0.06x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Singleton       | Small        | 109.548 ns | 1.1785 ns | 1.1024 ns | 109.544 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_MediatR_Object         | Request,Object         | Singleton       | Large        | 112.767 ns | 0.1669 ns | 0.1303 ns | 112.756 ns |      baseline |         |    1 | 0.0186 |     312 B |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Large        | 325.803 ns | 3.7515 ns | 3.5091 ns | 325.400 ns |  2.89x slower |   0.03x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Small        |  96.530 ns | 1.5440 ns | 1.4442 ns |  96.505 ns |  1.13x faster |   0.02x |    1 | 0.0095 |     160 B |  1.95x less |
| Request_MediatR_Object         | Request,Object         | Transient       | Small        | 108.765 ns | 1.4927 ns | 1.3962 ns | 109.516 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| Request_MediatR_Object         | Request,Object         | Transient       | Large        | 120.426 ns | 1.9912 ns | 1.8625 ns | 119.112 ns |      baseline |         |    1 | 0.0186 |     312 B |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Large        | 268.365 ns | 0.2215 ns | 0.1963 ns | 268.274 ns |  2.23x slower |   0.03x |    2 | 0.0095 |     160 B |  1.95x less |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Small        | 113.539 ns | 0.9080 ns | 0.7089 ns | 113.846 ns |  2.74x faster |   0.02x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Small        | 119.369 ns | 1.8841 ns | 1.7624 ns | 119.369 ns |  2.61x faster |   0.04x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Small        | 311.278 ns | 0.7062 ns | 0.5897 ns | 311.315 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Large        | 113.002 ns | 1.1009 ns | 1.0298 ns | 112.375 ns |  2.93x faster |   0.04x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Large        | 152.507 ns | 0.1584 ns | 0.1237 ns | 152.481 ns |  2.17x faster |   0.02x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Large        | 330.679 ns | 3.5016 ns | 3.2754 ns | 328.835 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Small        |  92.308 ns | 0.8510 ns | 0.7960 ns |  91.908 ns |  3.68x faster |   0.04x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Small        | 100.587 ns | 0.5589 ns | 0.5228 ns | 100.301 ns |  3.37x faster |   0.03x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Small        | 339.326 ns | 2.1874 ns | 2.0461 ns | 338.484 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Large        | 235.515 ns | 2.9891 ns | 2.7960 ns | 234.991 ns |  1.38x faster |   0.02x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Large        | 262.335 ns | 3.0393 ns | 2.8430 ns | 260.519 ns |  1.24x faster |   0.01x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Large        | 325.226 ns | 0.6448 ns | 0.5385 ns | 325.133 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Small        | 167.902 ns | 2.2234 ns | 2.0797 ns | 167.603 ns |  2.05x faster |   0.04x |    1 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Small        | 178.466 ns | 1.4968 ns | 1.2499 ns | 178.050 ns |  1.93x faster |   0.03x |    2 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Small        | 343.803 ns | 4.8654 ns | 4.5511 ns | 344.335 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Large        | 181.643 ns | 3.5462 ns | 3.9416 ns | 179.964 ns |  1.93x faster |   0.05x |    1 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Large        | 227.855 ns | 1.2069 ns | 2.0493 ns | 228.437 ns |  1.54x faster |   0.02x |    2 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Large        | 350.812 ns | 4.9857 ns | 4.6636 ns | 349.647 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Small        | 119.276 ns | 0.1763 ns | 0.1472 ns | 119.258 ns |  3.91x faster |   0.04x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Small        | 466.043 ns | 4.7191 ns | 4.4142 ns | 465.518 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Large        | 235.218 ns | 3.4846 ns | 3.2595 ns | 234.849 ns |  1.92x faster |   0.04x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Large        | 451.541 ns | 6.3266 ns | 5.9179 ns | 451.306 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Small        |  93.591 ns | 0.9147 ns | 0.8556 ns |  93.079 ns |  4.94x faster |   0.05x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Small        | 461.944 ns | 1.2989 ns | 1.1514 ns | 461.774 ns |      baseline |         |    2 | 0.0429 |     728 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Large        | 337.011 ns | 0.7899 ns | 0.6167 ns | 336.767 ns |  1.42x faster |   0.01x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Large        | 477.883 ns | 4.6405 ns | 4.3408 ns | 475.576 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Small        | 180.460 ns | 2.1876 ns | 2.0463 ns | 180.630 ns |  2.45x faster |   0.03x |    1 | 0.0148 |     248 B |  2.94x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Small        | 442.035 ns | 4.2266 ns | 3.9536 ns | 443.437 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Large        | 302.183 ns | 4.4428 ns | 4.1558 ns | 299.766 ns |  1.50x faster |   0.03x |    1 | 0.0148 |     248 B |  2.94x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Large        | 453.657 ns | 6.1587 ns | 5.7609 ns | 453.726 ns |      baseline |         |    2 | 0.0434 |     728 B |             |

## PR v4:

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.408
  [Host]          : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Scoped/False    : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Scoped/True     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Singleton/False : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Singleton/True  : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Transient/False : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  Transient/True  : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


```
| Method                         | Categories             | ServiceLifetime | Project type | Mean       | Error     | StdDev    | Ratio         | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------------------- |---------------- |------------- |-----------:|----------:|----------:|--------------:|--------:|-----:|-------:|----------:|------------:|
| ColdStart_IMediator            | ColdStart              | Scoped          | Small        | 191.108 ns | 1.9233 ns | 1.7991 ns |  1.09x faster |   0.03x |    1 | 0.0262 |     440 B |  1.33x less |
| ColdStart_MediatR              | ColdStart              | Scoped          | Small        | 207.842 ns | 4.2133 ns | 6.3063 ns |      baseline |         |    2 | 0.0348 |     584 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| ColdStart_IMediator            | ColdStart              | Scoped          | Large        | 199.753 ns | 2.4904 ns | 2.3296 ns |  1.05x faster |   0.02x |    1 | 0.0262 |     440 B |  1.33x less |
| ColdStart_MediatR              | ColdStart              | Scoped          | Large        | 209.313 ns | 3.1457 ns | 2.9425 ns |      baseline |         |    2 | 0.0348 |     584 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| ColdStart_IMediator            | ColdStart              | Singleton       | Small        |  23.042 ns | 0.1930 ns | 0.1806 ns |  4.53x faster |   0.07x |    1 |      - |         - |          NA |
| ColdStart_MediatR              | ColdStart              | Singleton       | Small        | 104.325 ns | 1.4136 ns | 1.3223 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| ColdStart_IMediator            | ColdStart              | Singleton       | Large        |  32.398 ns | 0.0827 ns | 0.0774 ns |  3.41x faster |   0.04x |    1 |      - |         - |          NA |
| ColdStart_MediatR              | ColdStart              | Singleton       | Large        | 110.396 ns | 1.4047 ns | 1.3140 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| ColdStart_IMediator            | ColdStart              | Transient       | Small        |  90.875 ns | 1.1758 ns | 1.0999 ns |  1.37x faster |   0.03x |    1 | 0.0076 |     128 B |  2.12x less |
| ColdStart_MediatR              | ColdStart              | Transient       | Small        | 124.471 ns | 2.3854 ns | 2.6514 ns |      baseline |         |    2 | 0.0162 |     272 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| ColdStart_IMediator            | ColdStart              | Transient       | Large        |  92.818 ns | 0.9045 ns | 0.8461 ns |  1.21x faster |   0.01x |    1 | 0.0076 |     128 B |  2.12x less |
| ColdStart_MediatR              | ColdStart              | Transient       | Large        | 112.736 ns | 0.5756 ns | 0.4494 ns |      baseline |         |    2 | 0.0162 |     272 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Initialization_IMediator       | Initialization         | Scoped          | Small        |  75.332 ns | 1.0846 ns | 1.0145 ns |  1.03x faster |   0.02x |    1 | 0.0210 |     352 B |  1.02x more |
| Initialization_MediatR         | Initialization         | Scoped          | Small        |  77.666 ns | 0.8895 ns | 0.7885 ns |      baseline |         |    1 | 0.0205 |     344 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Initialization_IMediator       | Initialization         | Scoped          | Large        |  74.107 ns | 0.9796 ns | 0.9163 ns |  1.01x faster |   0.02x |    1 | 0.0210 |     352 B |  1.02x more |
| Initialization_MediatR         | Initialization         | Scoped          | Large        |  74.857 ns | 0.8896 ns | 0.8321 ns |      baseline |         |    1 | 0.0205 |     344 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Initialization_IMediator       | Initialization         | Singleton       | Small        |   8.468 ns | 0.0901 ns | 0.0843 ns |  1.02x faster |   0.01x |    1 |      - |         - |          NA |
| Initialization_MediatR         | Initialization         | Singleton       | Small        |   8.631 ns | 0.0697 ns | 0.0652 ns |      baseline |         |    1 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Initialization_MediatR         | Initialization         | Singleton       | Large        |   8.589 ns | 0.0924 ns | 0.0864 ns |      baseline |         |    1 |      - |         - |          NA |
| Initialization_IMediator       | Initialization         | Singleton       | Large        |   8.876 ns | 0.0897 ns | 0.0839 ns |  1.03x slower |   0.01x |    1 |      - |         - |          NA |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Initialization_IMediator       | Initialization         | Transient       | Small        |  12.890 ns | 0.1539 ns | 0.1440 ns |  1.06x faster |   0.02x |    1 | 0.0024 |      40 B |  1.25x more |
| Initialization_MediatR         | Initialization         | Transient       | Small        |  13.714 ns | 0.1992 ns | 0.1864 ns |      baseline |         |    2 | 0.0019 |      32 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Initialization_IMediator       | Initialization         | Transient       | Large        |  12.094 ns | 0.1124 ns | 0.0938 ns |  1.08x faster |   0.02x |    1 | 0.0024 |      40 B |  1.25x more |
| Initialization_MediatR         | Initialization         | Transient       | Large        |  13.043 ns | 0.1766 ns | 0.1652 ns |      baseline |         |    2 | 0.0019 |      32 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Scoped          | Small        |  46.651 ns | 0.1959 ns | 0.1530 ns |  1.81x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Small        |  50.405 ns | 0.4565 ns | 0.4270 ns |  1.67x faster |   0.02x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Small        |  84.405 ns | 1.0722 ns | 1.0029 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Scoped          | Large        |  59.101 ns | 0.6971 ns | 0.6521 ns |  1.41x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Large        |  68.816 ns | 1.0477 ns | 0.9800 ns |  1.21x faster |   0.02x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Large        |  83.417 ns | 0.2888 ns | 0.2412 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Small        |  11.971 ns | 0.1627 ns | 0.1522 ns |  6.95x faster |   0.09x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Small        |  16.465 ns | 0.2040 ns | 0.1908 ns |  5.05x faster |   0.06x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Small        |  83.177 ns | 0.4193 ns | 0.3274 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Large        |  13.288 ns | 0.1249 ns | 0.1169 ns |  6.53x faster |   0.10x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Large        |  26.305 ns | 0.2834 ns | 0.2651 ns |  3.30x faster |   0.05x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Large        |  86.825 ns | 1.2913 ns | 1.2078 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Small        |  51.482 ns | 0.5381 ns | 0.4770 ns |  1.62x faster |   0.02x |    1 | 0.0033 |      56 B |  5.14x less |
| Notification_IMediator         | Notification,Concrete  | Transient       | Small        |  60.727 ns | 0.6603 ns | 0.6176 ns |  1.38x faster |   0.02x |    2 | 0.0033 |      56 B |  5.14x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Small        |  83.648 ns | 0.5735 ns | 0.4789 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Large        |  64.466 ns | 0.8896 ns | 0.8321 ns |  1.48x faster |   0.02x |    1 | 0.0033 |      56 B |  5.14x less |
| Notification_IMediator         | Notification,Concrete  | Transient       | Large        |  74.558 ns | 0.5502 ns | 0.5147 ns |  1.28x faster |   0.01x |    2 | 0.0033 |      56 B |  5.14x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Large        |  95.229 ns | 0.4075 ns | 0.3402 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Small        |  45.931 ns | 0.3986 ns | 0.3728 ns |  1.82x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Small        |  83.746 ns | 0.5583 ns | 0.5223 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Large        |  58.505 ns | 0.7331 ns | 0.6858 ns |  1.49x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Large        |  87.123 ns | 0.7565 ns | 0.7076 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Small        |  12.865 ns | 0.1803 ns | 0.1686 ns |  6.59x faster |   0.11x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Small        |  84.748 ns | 1.0428 ns | 0.9754 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Large        |  24.660 ns | 0.2674 ns | 0.2501 ns |  3.43x faster |   0.05x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Large        |  84.626 ns | 0.9467 ns | 0.8855 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Small        |  53.893 ns | 0.9813 ns | 0.9179 ns |  1.63x faster |   0.03x |    1 | 0.0033 |      56 B |  5.14x less |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Small        |  87.925 ns | 1.3078 ns | 1.2233 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Large        |  67.662 ns | 0.9635 ns | 0.9013 ns |  1.23x faster |   0.02x |    1 | 0.0033 |      56 B |  5.14x less |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Large        |  82.898 ns | 1.1181 ns | 1.0459 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Scoped          | Small        |  63.357 ns | 0.8961 ns | 0.8382 ns |  1.63x faster |   0.03x |    1 | 0.0038 |      64 B |  3.75x less |
| Request_IMediator              | Request,Concrete       | Scoped          | Small        |  68.325 ns | 0.8602 ns | 0.8047 ns |  1.52x faster |   0.02x |    2 | 0.0038 |      64 B |  3.75x less |
| Request_MediatR                | Request,Concrete       | Scoped          | Small        | 103.521 ns | 1.2670 ns | 1.1851 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Scoped          | Large        |  77.438 ns | 1.5781 ns | 1.5499 ns |  1.31x faster |   0.03x |    1 | 0.0038 |      64 B |  3.75x less |
| Request_IMediator              | Request,Concrete       | Scoped          | Large        |  80.193 ns | 1.1701 ns | 1.0945 ns |  1.27x faster |   0.02x |    1 | 0.0038 |      64 B |  3.75x less |
| Request_MediatR                | Request,Concrete       | Scoped          | Large        | 101.596 ns | 1.2459 ns | 1.1655 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Singleton       | Small        |   2.098 ns | 0.0424 ns | 0.0397 ns | 47.08x faster |   1.06x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Singleton       | Small        |  12.853 ns | 0.1698 ns | 0.1418 ns |  7.68x faster |   0.13x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Singleton       | Small        |  98.742 ns | 1.5317 ns | 1.3578 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Singleton       | Large        |  15.003 ns | 0.1314 ns | 0.1165 ns |  6.92x faster |   0.10x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Singleton       | Large        |  23.695 ns | 0.2688 ns | 0.2515 ns |  4.38x faster |   0.07x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Singleton       | Large        | 103.805 ns | 1.4662 ns | 1.3715 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Transient       | Small        |  43.560 ns | 0.1383 ns | 0.1226 ns |  2.22x faster |   0.03x |    1 | 0.0052 |      88 B |  2.73x less |
| Request_IMediator              | Request,Concrete       | Transient       | Small        |  54.606 ns | 0.6791 ns | 0.6353 ns |  1.77x faster |   0.03x |    2 | 0.0052 |      88 B |  2.73x less |
| Request_MediatR                | Request,Concrete       | Transient       | Small        |  96.665 ns | 1.3057 ns | 1.2213 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Transient       | Large        |  54.950 ns | 0.5451 ns | 0.5099 ns |  1.94x faster |   0.03x |    1 | 0.0052 |      88 B |  2.73x less |
| Request_IMediator              | Request,Concrete       | Transient       | Large        |  67.711 ns | 0.8576 ns | 0.8022 ns |  1.57x faster |   0.03x |    2 | 0.0052 |      88 B |  2.73x less |
| Request_MediatR                | Request,Concrete       | Transient       | Large        | 106.376 ns | 1.2919 ns | 1.2085 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Small        |  82.026 ns | 1.1236 ns | 1.0510 ns |  1.33x faster |   0.02x |    1 | 0.0038 |      64 B |  4.88x less |
| Request_MediatR_Object         | Request,Object         | Scoped          | Small        | 109.344 ns | 1.5044 ns | 1.4072 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Large        |  99.905 ns | 0.0873 ns | 0.0729 ns |  1.51x faster |   0.02x |    1 | 0.0038 |      64 B |  4.88x less |
| Request_MediatR_Object         | Request,Object         | Scoped          | Large        | 150.399 ns | 1.6683 ns | 1.5605 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Small        |  17.439 ns | 0.2418 ns | 0.2262 ns |  6.25x faster |   0.11x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Singleton       | Small        | 109.038 ns | 1.3856 ns | 1.2961 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Large        |  48.470 ns | 0.5124 ns | 0.4793 ns |  2.50x faster |   0.02x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Singleton       | Large        | 121.127 ns | 0.1850 ns | 0.1445 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Small        |  66.815 ns | 1.1142 ns | 1.0422 ns |  1.79x faster |   0.03x |    1 | 0.0052 |      88 B |  3.55x less |
| Request_MediatR_Object         | Request,Object         | Transient       | Small        | 119.806 ns | 1.3310 ns | 1.2450 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Large        |  94.433 ns | 1.9160 ns | 1.8817 ns |  1.22x faster |   0.03x |    1 | 0.0052 |      88 B |  3.55x less |
| Request_MediatR_Object         | Request,Object         | Transient       | Large        | 115.481 ns | 1.7855 ns | 1.6702 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Small        | 145.047 ns | 1.4091 ns | 1.3180 ns |  2.23x faster |   0.02x |    1 | 0.0091 |     152 B |  3.47x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Small        | 153.171 ns | 1.8964 ns | 1.7739 ns |  2.11x faster |   0.03x |    2 | 0.0091 |     152 B |  3.47x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Small        | 323.862 ns | 2.9231 ns | 2.2822 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Large        | 169.930 ns | 1.6638 ns | 1.5563 ns |  1.89x faster |   0.03x |    1 | 0.0091 |     152 B |  3.47x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Large        | 176.179 ns | 0.7354 ns | 0.5742 ns |  1.82x faster |   0.02x |    2 | 0.0091 |     152 B |  3.47x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Large        | 320.439 ns | 4.3165 ns | 4.0377 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Small        |  86.689 ns | 1.3073 ns | 1.2228 ns |  3.60x faster |   0.05x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Small        |  93.958 ns | 1.5775 ns | 1.4756 ns |  3.32x faster |   0.05x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Small        | 311.944 ns | 2.1958 ns | 2.0539 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Large        | 106.205 ns | 1.1738 ns | 1.0980 ns |  3.20x faster |   0.05x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Large        | 108.389 ns | 1.0138 ns | 0.9483 ns |  3.14x faster |   0.04x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Large        | 339.977 ns | 3.8415 ns | 3.5933 ns |      baseline |         |    2 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Small        | 136.529 ns | 1.7344 ns | 1.6224 ns |  2.39x faster |   0.03x |    1 | 0.0105 |     176 B |  3.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Small        | 148.985 ns | 3.0151 ns | 2.8203 ns |  2.19x faster |   0.04x |    2 | 0.0105 |     176 B |  3.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Small        | 326.418 ns | 3.1418 ns | 2.9388 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Large        | 159.011 ns | 1.7273 ns | 1.6157 ns |  2.05x faster |   0.02x |    1 | 0.0105 |     176 B |  3.00x less |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Large        | 160.477 ns | 1.3211 ns | 1.2358 ns |  2.04x faster |   0.02x |    1 | 0.0105 |     176 B |  3.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Large        | 326.714 ns | 0.4525 ns | 0.3533 ns |      baseline |         |    2 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Small        | 146.923 ns | 2.0482 ns | 1.9159 ns |  3.26x faster |   0.06x |    1 | 0.0091 |     152 B |  4.79x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Small        | 478.365 ns | 6.4954 ns | 6.0758 ns |      baseline |         |    2 | 0.0429 |     728 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Large        | 270.108 ns | 1.7536 ns | 1.4643 ns |  1.67x faster |   0.02x |    1 | 0.0210 |     352 B |  2.07x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Large        | 450.637 ns | 4.8082 ns | 4.4976 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Small        |  88.213 ns | 0.8842 ns | 0.8271 ns |  5.10x faster |   0.05x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Small        | 449.921 ns | 1.6890 ns | 1.3186 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Large        | 220.723 ns | 2.1260 ns | 1.8847 ns |  2.01x faster |   0.02x |    1 | 0.0162 |     272 B |  2.68x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Large        | 443.086 ns | 0.8624 ns | 0.7645 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Small        | 135.270 ns | 1.6612 ns | 1.5539 ns |  3.27x faster |   0.05x |    1 | 0.0105 |     176 B |  4.14x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Small        | 441.972 ns | 5.2229 ns | 4.8855 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |           |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Large        | 282.982 ns | 3.1931 ns | 2.9868 ns |  1.54x faster |   0.02x |    1 | 0.0224 |     376 B |  1.94x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Large        | 435.873 ns | 1.3962 ns | 1.1659 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
