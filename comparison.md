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
| Method                         | Categories             | ServiceLifetime | Project type | Mean       | Error     | StdDev     | Median     | Ratio         | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------------------- |---------------- |------------- |-----------:|----------:|-----------:|-----------:|--------------:|--------:|-----:|-------:|----------:|------------:|
| Notification_Mediator          | Notification,Concrete  | Scoped          | Small        |  63.650 ns | 1.2376 ns |  1.3242 ns |  63.233 ns |  1.63x faster |   0.04x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Small        |  71.565 ns | 1.2385 ns |  1.1585 ns |  71.244 ns |  1.45x faster |   0.03x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Small        | 103.564 ns | 1.6789 ns |  1.5705 ns | 103.206 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Scoped          | Large        |  61.661 ns | 1.2340 ns |  1.2119 ns |  61.497 ns |  1.56x faster |   0.05x |    1 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Large        |  95.940 ns | 1.8718 ns |  2.2987 ns |  95.514 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Large        |  97.737 ns | 1.8685 ns |  1.7478 ns |  96.915 ns |  1.02x slower |   0.03x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Small        |  21.215 ns | 0.3313 ns |  0.3099 ns |  21.283 ns |  4.50x faster |   0.10x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Small        |  28.035 ns | 0.5734 ns |  0.7252 ns |  27.858 ns |  3.41x faster |   0.10x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Small        |  95.553 ns | 1.7070 ns |  1.5968 ns |  95.352 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Large        | 107.454 ns | 2.1291 ns |  1.9916 ns | 106.820 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Large        | 298.797 ns | 5.8715 ns |  9.4814 ns | 299.612 ns |  2.78x slower |   0.10x |    2 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Large        | 320.614 ns | 6.3153 ns |  6.2024 ns | 321.815 ns |  2.98x slower |   0.08x |    3 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Small        |  72.056 ns | 1.3613 ns |  1.2734 ns |  71.674 ns |  1.32x faster |   0.03x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_IMediator         | Notification,Concrete  | Transient       | Small        |  74.030 ns | 1.2358 ns |  1.1559 ns |  74.374 ns |  1.29x faster |   0.03x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Small        |  95.315 ns | 1.8723 ns |  1.8388 ns |  95.666 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Large        |  70.210 ns | 1.2979 ns |  1.2141 ns |  70.333 ns |  1.48x faster |   0.03x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Large        | 103.580 ns | 1.9111 ns |  1.7876 ns | 103.015 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
| Notification_IMediator         | Notification,Concrete  | Transient       | Large        | 115.882 ns | 2.3300 ns |  2.5898 ns | 115.076 ns |  1.12x slower |   0.03x |    3 | 0.0048 |      80 B |  3.60x less |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Small        |  63.433 ns | 1.2726 ns |  1.3069 ns |  63.417 ns |  1.47x faster |   0.04x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Small        |  93.210 ns | 1.6411 ns |  1.5351 ns |  93.469 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Large        |  95.679 ns | 1.8820 ns |  1.8484 ns |  94.919 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Large        |  97.616 ns | 1.9145 ns |  1.7908 ns |  97.154 ns |  1.02x slower |   0.03x |    1 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Small        |  22.379 ns | 0.3780 ns |  0.3536 ns |  22.340 ns |  4.07x faster |   0.11x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Small        |  91.109 ns | 1.8416 ns |  2.1208 ns |  91.217 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Large        |  95.287 ns | 1.8559 ns |  1.6452 ns |  94.988 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Large        | 316.709 ns | 6.2369 ns | 10.7584 ns | 313.927 ns |  3.32x slower |   0.12x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Small        |  73.169 ns | 1.4473 ns |  1.7774 ns |  72.689 ns |  1.32x faster |   0.04x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Small        |  96.536 ns | 1.7321 ns |  1.6202 ns |  96.507 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Large        |  96.158 ns | 1.8316 ns |  1.6236 ns |  96.437 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Large        | 120.339 ns | 2.3678 ns |  2.5335 ns | 119.657 ns |  1.25x slower |   0.03x |    2 | 0.0048 |      80 B |  3.60x less |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Scoped          | Small        |  26.536 ns | 0.5232 ns |  0.4894 ns |  26.475 ns |  4.16x faster |   0.13x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Scoped          | Small        |  40.032 ns | 0.7233 ns |  0.6766 ns |  39.926 ns |  2.76x faster |   0.09x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Scoped          | Small        | 110.457 ns | 2.2051 ns |  2.9437 ns | 110.136 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Scoped          | Large        |  25.059 ns | 0.4994 ns |  0.4671 ns |  24.997 ns |  5.13x faster |   0.15x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Scoped          | Large        |  39.402 ns | 0.7996 ns |  0.9208 ns |  39.311 ns |  3.26x faster |   0.11x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Scoped          | Large        | 128.570 ns | 2.5887 ns |  3.0816 ns | 127.915 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Singleton       | Small        |   8.448 ns | 0.1932 ns |  0.1807 ns |   8.399 ns | 12.49x faster |   0.34x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Singleton       | Small        |  19.209 ns | 0.2585 ns |  0.2291 ns |  19.138 ns |  5.49x faster |   0.12x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Singleton       | Small        | 105.428 ns | 2.1234 ns |  1.9863 ns | 105.359 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_MediatR                | Request,Concrete       | Singleton       | Large        | 103.088 ns | 1.6802 ns |  1.5717 ns | 103.087 ns |      baseline |         |    1 | 0.0143 |     240 B |             |
| Request_Mediator               | Request,Concrete       | Singleton       | Large        | 147.735 ns | 2.9170 ns |  3.1211 ns | 146.431 ns |  1.43x slower |   0.04x |    2 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Singleton       | Large        | 150.167 ns | 2.9901 ns |  8.1852 ns | 145.385 ns |  1.46x slower |   0.08x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Transient       | Small        |  91.102 ns | 1.8482 ns |  1.9776 ns |  90.609 ns |  1.10x faster |   0.03x |    1 | 0.0095 |     160 B |  1.50x less |
| Request_IMediator              | Request,Concrete       | Transient       | Small        |  96.655 ns | 1.9682 ns |  2.8227 ns |  95.959 ns |  1.03x faster |   0.04x |    2 | 0.0095 |     160 B |  1.50x less |
| Request_MediatR                | Request,Concrete       | Transient       | Small        |  99.871 ns | 2.0530 ns |  2.0164 ns |  99.548 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Transient       | Large        |  88.598 ns | 1.7909 ns |  1.6752 ns |  89.197 ns |  1.28x faster |   0.03x |    1 | 0.0095 |     160 B |  1.50x less |
| Request_IMediator              | Request,Concrete       | Transient       | Large        | 104.396 ns | 1.2761 ns |  1.1937 ns | 104.263 ns |  1.08x faster |   0.02x |    2 | 0.0095 |     160 B |  1.50x less |
| Request_MediatR                | Request,Concrete       | Transient       | Large        | 113.237 ns | 2.2574 ns |  2.1116 ns | 112.879 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Small        |  47.064 ns | 0.7497 ns |  0.7012 ns |  47.047 ns |  2.53x faster |   0.05x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Scoped          | Small        | 118.863 ns | 1.7817 ns |  1.6666 ns | 118.066 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_MediatR_Object         | Request,Object         | Scoped          | Large        | 131.628 ns | 2.5909 ns |  2.9836 ns | 130.162 ns |      baseline |         |    1 | 0.0186 |     312 B |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Large        | 217.822 ns | 3.8573 ns |  3.7884 ns | 217.100 ns |  1.66x slower |   0.05x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Small        |  25.342 ns | 0.5351 ns |  0.5725 ns |  25.288 ns |  4.74x faster |   0.11x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Singleton       | Small        | 119.963 ns | 1.2499 ns |  1.0437 ns | 119.783 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_MediatR_Object         | Request,Object         | Singleton       | Large        | 126.347 ns | 2.5364 ns |  2.7139 ns | 125.866 ns |      baseline |         |    1 | 0.0186 |     312 B |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Large        | 331.638 ns | 6.5500 ns |  6.4329 ns | 330.217 ns |  2.63x slower |   0.07x |    2 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Small        | 111.246 ns | 2.0175 ns |  1.8872 ns | 111.237 ns |  1.16x faster |   0.03x |    1 | 0.0095 |     160 B |  1.95x less |
| Request_MediatR_Object         | Request,Object         | Transient       | Small        | 128.878 ns | 2.4158 ns |  2.2598 ns | 128.635 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| Request_MediatR_Object         | Request,Object         | Transient       | Large        | 137.583 ns | 2.7935 ns |  3.3255 ns | 136.115 ns |      baseline |         |    1 | 0.0186 |     312 B |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Large        | 297.160 ns | 5.9987 ns |  6.1602 ns | 293.321 ns |  2.16x slower |   0.07x |    2 | 0.0095 |     160 B |  1.95x less |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Small        | 118.244 ns | 1.2997 ns |  1.0853 ns | 118.018 ns |  2.94x faster |   0.05x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Small        | 124.855 ns | 1.1346 ns |  1.0613 ns | 124.870 ns |  2.79x faster |   0.05x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Small        | 347.982 ns | 5.7336 ns |  5.0827 ns | 348.261 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Large        | 121.440 ns | 2.4102 ns |  2.4751 ns | 120.270 ns |  2.86x faster |   0.07x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Large        | 168.129 ns | 3.3827 ns |  3.8955 ns | 166.885 ns |  2.06x faster |   0.05x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Large        | 346.775 ns | 5.4294 ns |  5.0787 ns | 346.491 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Small        | 100.069 ns | 1.9757 ns |  2.1139 ns |  99.590 ns |  3.49x faster |   0.10x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Small        | 105.450 ns | 1.0848 ns |  1.0148 ns | 105.411 ns |  3.31x faster |   0.07x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Small        | 349.128 ns | 6.6330 ns |  6.5145 ns | 346.536 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Large        | 249.495 ns | 4.1634 ns |  3.8944 ns | 251.817 ns |  1.43x faster |   0.04x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Large        | 263.863 ns | 5.2675 ns |  5.6362 ns | 262.128 ns |  1.35x faster |   0.04x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Large        | 356.883 ns | 7.0991 ns |  7.2902 ns | 356.612 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Small        | 178.785 ns | 2.8359 ns |  2.5140 ns | 178.241 ns |  2.12x faster |   0.05x |    1 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Small        | 189.243 ns | 3.7139 ns |  4.5611 ns | 188.787 ns |  2.00x faster |   0.06x |    2 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Small        | 378.367 ns | 7.4784 ns |  7.3448 ns | 376.111 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Large        | 193.521 ns | 3.5411 ns |  3.3124 ns | 194.187 ns |  1.87x faster |   0.05x |    1 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Large        | 241.678 ns | 4.6415 ns |  5.5254 ns | 242.296 ns |  1.50x faster |   0.04x |    2 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Large        | 361.622 ns | 7.0521 ns |  7.2420 ns | 360.588 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Small        | 121.994 ns | 2.4350 ns |  2.8987 ns | 120.765 ns |  3.80x faster |   0.10x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Small        | 463.242 ns | 6.4940 ns |  6.0745 ns | 464.343 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Large        | 248.420 ns | 4.8121 ns |  5.3486 ns | 246.165 ns |  2.06x faster |   0.05x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Large        | 510.685 ns | 7.0499 ns |  6.5945 ns | 510.296 ns |      baseline |         |    2 | 0.0429 |     728 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Small        |  99.508 ns | 1.9539 ns |  2.0065 ns |  99.070 ns |  4.88x faster |   0.13x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Small        | 485.276 ns | 9.3141 ns |  8.7124 ns | 482.259 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Large        | 352.458 ns | 4.4891 ns |  3.9794 ns | 352.509 ns |  1.38x faster |   0.02x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Large        | 487.277 ns | 4.5874 ns |  4.2910 ns | 486.873 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Small        | 189.994 ns | 2.2687 ns |  2.1222 ns | 190.210 ns |  2.58x faster |   0.04x |    1 | 0.0148 |     248 B |  2.94x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Small        | 489.655 ns | 7.1247 ns |  6.6644 ns | 490.497 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |            |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Large        | 320.107 ns | 6.3727 ns |  7.0833 ns | 318.208 ns |  1.57x faster |   0.05x |    1 | 0.0148 |     248 B |  2.94x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Large        | 501.886 ns | 9.7482 ns | 13.3435 ns | 495.142 ns |      baseline |         |    2 | 0.0429 |     728 B |             |

## PR:

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
| Method                         | Categories             | ServiceLifetime | Project type | Mean       | Error     | StdDev     | Ratio         | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------------------- |---------------- |------------- |-----------:|----------:|-----------:|--------------:|--------:|-----:|-------:|----------:|------------:|
| Notification_Mediator          | Notification,Concrete  | Scoped          | Small        |  63.485 ns | 1.2534 ns |  1.2872 ns |  1.52x faster |   0.05x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Small        |  74.408 ns | 0.8240 ns |  0.7707 ns |  1.29x faster |   0.04x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Small        |  96.191 ns | 1.9155 ns |  2.8078 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Scoped          | Large        |  66.271 ns | 1.3441 ns |  1.5479 ns |  1.42x faster |   0.04x |    1 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Scoped          | Large        |  94.196 ns | 1.2186 ns |  1.0803 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
| Notification_IMediator         | Notification,Concrete  | Scoped          | Large        | 104.736 ns | 1.3331 ns |  1.1132 ns |  1.11x slower |   0.02x |    3 |      - |         - |          NA |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Small        |  11.363 ns | 0.2487 ns |  0.2661 ns |  8.35x faster |   0.23x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Small        |  15.482 ns | 0.3243 ns |  0.3330 ns |  6.13x faster |   0.16x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Small        |  94.842 ns | 1.7223 ns |  1.6111 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Singleton       | Large        |  12.666 ns | 0.2506 ns |  0.2886 ns |  8.55x faster |   0.21x |    1 |      - |         - |          NA |
| Notification_IMediator         | Notification,Concrete  | Singleton       | Large        |  38.851 ns | 0.6204 ns |  0.5803 ns |  2.79x faster |   0.05x |    2 |      - |         - |          NA |
| Notification_MediatR           | Notification,Concrete  | Singleton       | Large        | 108.183 ns | 1.6476 ns |  1.3758 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Small        |  64.699 ns | 1.2923 ns |  1.3271 ns |  1.48x faster |   0.04x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_IMediator         | Notification,Concrete  | Transient       | Small        |  70.882 ns | 1.1926 ns |  1.0572 ns |  1.35x faster |   0.03x |    2 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Small        |  95.809 ns | 1.8809 ns |  1.7594 ns |      baseline |         |    3 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_Mediator          | Notification,Concrete  | Transient       | Large        |  66.046 ns | 1.2924 ns |  1.3272 ns |  1.51x faster |   0.04x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR           | Notification,Concrete  | Transient       | Large        |  99.909 ns | 1.9814 ns |  2.1201 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
| Notification_IMediator         | Notification,Concrete  | Transient       | Large        | 103.073 ns | 1.7448 ns |  1.6321 ns |  1.03x slower |   0.03x |    2 | 0.0048 |      80 B |  3.60x less |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Small        |  63.879 ns | 1.2262 ns |  1.1470 ns |  1.43x faster |   0.03x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Small        |  91.384 ns | 1.4998 ns |  1.4029 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Scoped          | Large        |  93.490 ns | 0.7080 ns |  0.6622 ns |  1.11x faster |   0.02x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Scoped          | Large        | 104.106 ns | 2.0374 ns |  2.0010 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Small        |  12.566 ns | 0.2331 ns |  0.1947 ns |  7.69x faster |   0.17x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Small        |  96.562 ns | 1.8520 ns |  1.6417 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Singleton       | Large        |  35.036 ns | 0.6989 ns |  0.7478 ns |  2.63x faster |   0.07x |    1 |      - |         - |          NA |
| Notification_MediatR_Object    | Notification,Object    | Singleton       | Large        |  91.937 ns | 1.8389 ns |  1.7201 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Small        |  73.355 ns | 1.4837 ns |  1.5236 ns |  1.27x faster |   0.04x |    1 | 0.0048 |      80 B |  3.60x less |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Small        |  93.146 ns | 1.8602 ns |  2.4833 ns |      baseline |         |    2 | 0.0172 |     288 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Notification_MediatR_Object    | Notification,Object    | Transient       | Large        |  91.782 ns | 1.6998 ns |  1.5900 ns |      baseline |         |    1 | 0.0172 |     288 B |             |
| Notification_IMediator_Object  | Notification,Object    | Transient       | Large        | 113.566 ns | 1.1357 ns |  0.8867 ns |  1.24x slower |   0.02x |    2 | 0.0048 |      80 B |  3.60x less |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Scoped          | Small        |  25.102 ns | 0.2725 ns |  0.2549 ns |  3.91x faster |   0.08x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Scoped          | Small        |  38.241 ns | 0.6528 ns |  0.5787 ns |  2.57x faster |   0.06x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Scoped          | Small        |  98.119 ns | 1.8969 ns |  1.7744 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator              | Request,Concrete       | Scoped          | Large        |  19.400 ns | 0.3687 ns |  0.3449 ns |  5.82x faster |   0.14x |    1 |      - |         - |          NA |
| Request_Mediator               | Request,Concrete       | Scoped          | Large        |  26.441 ns | 0.5676 ns |  0.6073 ns |  4.27x faster |   0.12x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Scoped          | Large        | 112.937 ns | 1.9441 ns |  1.9094 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Singleton       | Small        |   1.738 ns | 0.0659 ns |  0.0617 ns | 56.98x faster |   2.15x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Singleton       | Small        |  13.604 ns | 0.3121 ns |  0.3205 ns |  7.27x faster |   0.20x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Singleton       | Small        |  98.881 ns | 1.6079 ns |  1.5040 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Singleton       | Large        |   4.429 ns | 0.0866 ns |  0.0962 ns | 24.28x faster |   0.89x |    1 |      - |         - |          NA |
| Request_IMediator              | Request,Concrete       | Singleton       | Large        |  24.990 ns | 0.4896 ns |  0.5638 ns |  4.30x faster |   0.16x |    2 |      - |         - |          NA |
| Request_MediatR                | Request,Concrete       | Singleton       | Large        | 107.467 ns | 2.1997 ns |  3.2924 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_Mediator               | Request,Concrete       | Transient       | Small        |  85.011 ns | 1.6106 ns |  1.5065 ns |  1.17x faster |   0.03x |    1 | 0.0095 |     160 B |  1.50x less |
| Request_IMediator              | Request,Concrete       | Transient       | Small        |  96.480 ns | 1.9178 ns |  2.4254 ns |  1.03x faster |   0.03x |    2 | 0.0095 |     160 B |  1.50x less |
| Request_MediatR                | Request,Concrete       | Transient       | Small        |  99.498 ns | 2.0423 ns |  2.0058 ns |      baseline |         |    2 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator              | Request,Concrete       | Transient       | Large        |  19.320 ns | 0.2965 ns |  0.2476 ns |  5.22x faster |   0.11x |    1 |      - |         - |          NA |
| Request_Mediator               | Request,Concrete       | Transient       | Large        |  87.287 ns | 1.4251 ns |  1.3330 ns |  1.15x faster |   0.03x |    2 | 0.0095 |     160 B |  1.50x less |
| Request_MediatR                | Request,Concrete       | Transient       | Large        | 100.751 ns | 2.0096 ns |  1.7815 ns |      baseline |         |    3 | 0.0143 |     240 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Small        |  48.640 ns | 0.8360 ns |  0.7820 ns |  2.39x faster |   0.04x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Scoped          | Small        | 116.277 ns | 0.5105 ns |  0.4263 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Scoped          | Large        |  45.328 ns | 0.9510 ns |  1.0571 ns |  3.02x faster |   0.10x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Scoped          | Large        | 136.630 ns | 2.7286 ns |  3.5479 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Small        |  20.275 ns | 0.3051 ns |  0.2854 ns |  5.91x faster |   0.15x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Singleton       | Small        | 119.896 ns | 2.3457 ns |  2.5099 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Singleton       | Large        |  54.802 ns | 1.0809 ns |  1.7455 ns |  2.43x faster |   0.09x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Singleton       | Large        | 132.803 ns | 2.6774 ns |  2.5045 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Small        | 107.659 ns | 2.1614 ns |  2.1228 ns |  1.08x faster |   0.03x |    1 | 0.0095 |     160 B |  1.95x less |
| Request_MediatR_Object         | Request,Object         | Transient       | Small        | 116.669 ns | 1.9746 ns |  1.8471 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| Request_IMediator_Object       | Request,Object         | Transient       | Large        |  44.813 ns | 0.9144 ns |  0.8980 ns |  2.81x faster |   0.08x |    1 |      - |         - |          NA |
| Request_MediatR_Object         | Request,Object         | Transient       | Large        | 126.099 ns | 2.5121 ns |  2.6879 ns |      baseline |         |    2 | 0.0186 |     312 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Small        | 119.201 ns | 2.3604 ns |  3.0691 ns |  2.94x faster |   0.09x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Small        | 126.125 ns | 2.4953 ns |  3.1557 ns |  2.77x faster |   0.08x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Small        | 349.677 ns | 5.9816 ns |  5.3025 ns |      baseline |         |    2 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator        | StreamRequest,Concrete | Scoped          | Large        | 112.975 ns | 2.2710 ns |  2.3322 ns |  3.32x faster |   0.09x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_Mediator         | StreamRequest,Concrete | Scoped          | Large        | 116.220 ns | 0.8834 ns |  0.7831 ns |  3.23x faster |   0.07x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Scoped          | Large        | 375.189 ns | 7.1950 ns |  7.3887 ns |      baseline |         |    2 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Small        |  94.050 ns | 1.0930 ns |  1.0224 ns |  3.79x faster |   0.09x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Small        |  97.253 ns | 0.9923 ns |  0.9282 ns |  3.66x faster |   0.08x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Small        | 356.224 ns | 7.1318 ns |  7.3239 ns |      baseline |         |    2 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Singleton       | Large        |  93.540 ns | 1.0550 ns |  0.9869 ns |  3.92x faster |   0.09x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Singleton       | Large        | 114.907 ns | 1.9252 ns |  1.8008 ns |  3.19x faster |   0.08x |    2 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Singleton       | Large        | 366.819 ns | 7.3678 ns |  7.8835 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Small        | 185.542 ns | 3.6972 ns |  4.9357 ns |  1.89x faster |   0.06x |    1 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Small        | 188.416 ns | 3.6981 ns |  4.9369 ns |  1.86x faster |   0.06x |    1 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Small        | 349.936 ns | 6.2381 ns |  5.8351 ns |      baseline |         |    2 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator        | StreamRequest,Concrete | Transient       | Large        | 111.945 ns | 2.1858 ns |  2.4295 ns |  3.37x faster |   0.07x |    1 | 0.0052 |      88 B |  6.00x less |
| StreamRequest_Mediator         | StreamRequest,Concrete | Transient       | Large        | 194.497 ns | 1.2999 ns |  1.1523 ns |  1.94x faster |   0.02x |    2 | 0.0148 |     248 B |  2.13x less |
| StreamRequest_MediatR          | StreamRequest,Concrete | Transient       | Large        | 377.624 ns | 2.7023 ns |  2.5278 ns |      baseline |         |    3 | 0.0315 |     528 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Small        | 120.146 ns | 1.2199 ns |  0.9524 ns |  3.98x faster |   0.12x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Small        | 478.622 ns | 8.8669 ns | 14.3183 ns |      baseline |         |    2 | 0.0429 |     728 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Scoped          | Large        | 220.121 ns | 3.4990 ns |  3.2730 ns |  2.18x faster |   0.05x |    1 | 0.0162 |     272 B |  2.68x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Scoped          | Large        | 479.986 ns | 8.7379 ns |  9.7122 ns |      baseline |         |    2 | 0.0429 |     728 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Small        |  93.743 ns | 0.9773 ns |  0.9142 ns |  5.07x faster |   0.06x |    1 | 0.0052 |      88 B |  8.27x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Small        | 475.179 ns | 5.0837 ns |  4.2451 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Singleton       | Large        | 222.400 ns | 2.1004 ns |  1.9647 ns |  2.22x faster |   0.03x |    1 | 0.0162 |     272 B |  2.68x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Singleton       | Large        | 493.513 ns | 5.5316 ns |  5.1742 ns |      baseline |         |    2 | 0.0429 |     728 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Small        | 189.253 ns | 3.6497 ns |  4.6157 ns |  2.50x faster |   0.06x |    1 | 0.0148 |     248 B |  2.94x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Small        | 473.284 ns | 4.3177 ns |  4.0387 ns |      baseline |         |    2 | 0.0434 |     728 B |             |
|                                |                        |                 |              |            |           |            |               |         |      |        |           |             |
| StreamRequest_IMediator_Object | StreamRequest,Object   | Transient       | Large        | 219.813 ns | 2.5918 ns |  2.1642 ns |  2.15x faster |   0.04x |    1 | 0.0162 |     272 B |  2.68x less |
| StreamRequest_MediatR_Object   | StreamRequest,Object   | Transient       | Large        | 472.920 ns | 8.1787 ns |  7.2502 ns |      baseline |         |    2 | 0.0434 |     728 B |             |


