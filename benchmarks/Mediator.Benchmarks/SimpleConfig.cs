using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;

namespace Mediator.Benchmarks;

internal sealed class SimpleConfig : ManualConfig
{
    public SimpleConfig()
    {
        SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
        AddColumn(RankColumn.Arabic);
        Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared);
        AddDiagnoser(MemoryDiagnoser.Default);
    }
}
