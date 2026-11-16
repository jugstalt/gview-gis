```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.9711/24H2/2024Update/HudsonValley)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 11.0.100
  [Host]   : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  ShortRun : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                      | Vertices | Mean          | Error        | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |--------- |--------------:|-------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| **Clipper**                     | **8**        |  **3,651.715 ns** | **1,862.013 ns** | **102.0632 ns** | **1.001** |    **0.03** | **2.1935** | **0.0191** |   **13776 B** |        **1.00** |
| Bridge                      | 8        |  1,724.839 ns | 2,210.387 ns | 121.1588 ns | 0.473 |    0.03 | 0.9003 | 0.0019 |    5648 B |        0.41 |
| OverlaysClip_GViewFactory   | 8        |  1,654.942 ns |   591.117 ns |  32.4011 ns | 0.453 |    0.01 | 0.9003 | 0.0019 |    5648 B |        0.41 |
| OverlaysClip_DefaultFactory | 8        |            NA |           NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| EnvelopeIntersects          | 8        |      6.520 ns |     2.944 ns |   0.1614 ns | 0.002 |    0.00 |      - |      - |         - |        0.00 |
|                             |          |               |              |             |       |         |        |        |           |             |
| **Clipper**                     | **50**       | **13,611.123 ns** | **2,298.326 ns** | **125.9790 ns** | **1.000** |    **0.01** | **7.3853** | **0.3204** |   **46360 B** |        **1.00** |
| Bridge                      | 50       |  3,952.043 ns |   922.287 ns |  50.5537 ns | 0.290 |    0.00 | 1.7624 | 0.0229 |   11072 B |        0.24 |
| OverlaysClip_GViewFactory   | 50       |  3,947.310 ns | 1,929.268 ns | 105.7497 ns | 0.290 |    0.01 | 1.7624 | 0.0229 |   11072 B |        0.24 |
| OverlaysClip_DefaultFactory | 50       |            NA |           NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| EnvelopeIntersects          | 50       |      6.146 ns |     4.427 ns |   0.2427 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |

Benchmarks with issues:
  SmallClipProfileBenchmarks.OverlaysClip_DefaultFactory: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3) [Vertices=8]
  SmallClipProfileBenchmarks.OverlaysClip_DefaultFactory: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3) [Vertices=50]
