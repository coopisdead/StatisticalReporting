using StatisticalReporting.Models;

namespace StatisticalReporting.Interfaces;

public interface ILogParser
{
    IEnumerable<LogEntry> Parse(string filePath);
}