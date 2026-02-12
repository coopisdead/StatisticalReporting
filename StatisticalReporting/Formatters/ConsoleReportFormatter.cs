using StatisticalReporting.Interfaces;
using StatisticalReporting.Models;

namespace StatisticalReporting.Formatters;

public class ConsoleReportFormatter : IReportFormatter
{
    public void Format(IEnumerable<DimensionReport> reports, TextWriter output)
    {
        foreach (var report in reports)
        {
            output.WriteLine($"{report.DimensionName}:");

            foreach (var entry in report.Entries)
            {
                output.WriteLine($"  {entry.Label} {entry.Percentage:F2}%");
            }

            output.WriteLine();
        }
    }
}