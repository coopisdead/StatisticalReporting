using StatisticalReporting.Models;

namespace StatisticalReporting.Interfaces;

public interface IDimensionExtractor
{
    string DimensionName { get; }
    string Extract(LogEntry entry);
}