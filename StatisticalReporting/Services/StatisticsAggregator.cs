using StatisticalReporting.Interfaces;
using StatisticalReporting.Models;

namespace StatisticalReporting.Services;

public class StatisticsAggregator
{
    private const double DefaultThreshold = 1.0;
    private const string OtherLabel = "Other";
    private const string UnknownLabel = "Unknown";

    public DimensionReport Aggregate(IList<LogEntry> entries, IDimensionExtractor extractor, double threshold = DefaultThreshold)
    {
        var total = entries.Count;

        var allResults = entries
            .GroupBy(extractor.Extract)
            .Select(group =>
            {
                var count = group.Count();
                var percentage = CalculatePercentage(count, total);
                return new DimensionEntry(Label: group.Key, Percentage: percentage);
            })
            .ToList();

        var aboveThreshold = allResults
            .Where(e => e.Percentage >= threshold || e.Label == UnknownLabel)
            .OrderByDescending(e => e.Percentage)
            .ToList();

        var belowThreshold = allResults
            .Where(e => e.Percentage < threshold && e.Label != UnknownLabel)
            .Sum(e => e.Percentage);

        if (belowThreshold > 0)
            aboveThreshold.Add(new DimensionEntry(OtherLabel, Math.Round(belowThreshold, 2)));

        return new DimensionReport(extractor.DimensionName, aboveThreshold);
    }

    private static double CalculatePercentage(int count, int total)
    {
        const int decimalPlaces = 2;
        return Math.Round((double)count / total * 100, decimalPlaces);
    }
}