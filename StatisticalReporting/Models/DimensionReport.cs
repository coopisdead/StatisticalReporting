namespace StatisticalReporting.Models;

public record DimensionReport(string DimensionName, IReadOnlyList<DimensionEntry> Entries);

public record DimensionEntry(string Label, double Percentage);