using StatisticalReporting.Models;

namespace StatisticalReporting.Interfaces;

public interface IReportFormatter
{
    void Format(IEnumerable<DimensionReport> reports, TextWriter output);
}