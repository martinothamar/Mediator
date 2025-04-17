using BenchmarkDotNet.Running;

namespace Mediator.Benchmarks;

public sealed class CustomColumn : IColumn
{
    private readonly Func<Summary, BenchmarkCase, string> _getTag;

    public string Id { get; }
    public string ColumnName { get; }

    public CustomColumn(string columnName, Func<Summary, BenchmarkCase, string> getTag)
    {
        _getTag = getTag;
        ColumnName = columnName;
        Id = nameof(CustomColumn) + "." + ColumnName;
    }

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => _getTag(summary, benchmarkCase);

    public bool IsAvailable(Summary summary) => true;

    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Params;
    public int PriorityInCategory => 0;
    public bool IsNumeric => false;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => $"Custom '{ColumnName}' tag column";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
        GetValue(summary, benchmarkCase);

    public override string ToString() => ColumnName;
}
