using StatisticalReporting.Interfaces;
using StatisticalReporting.Parsers;
using StatisticalReporting.Extractors;
using StatisticalReporting.Services;
using StatisticalReporting.Formatters;

const string logFilePath = "Resources/apache_log.txt";
const string geoIpDatabasePath = "Resources/GeoLite2-Country_20260210/GeoLite2-Country.mmdb";

try
{
    // Parse the log file
    var parser = new ApacheLogParser();
    var entries = parser.Parse(logFilePath).ToList();// Generating the entire list immediately, For bigger inputs, this would reqiure streaming.

    if (parser.SkippedLines > 0)
        Console.WriteLine($"Warning: Skipped {parser.SkippedLines} unparseable lines.\n");

    // Extract fields + aggregate results
    var uaService = new UserAgentService();
    var extractors = new List<IDimensionExtractor>
    {
        new CountryExtractor(geoIpDatabasePath),
        new OsExtractor(uaService),
        new BrowserExtractor(uaService)
    };

    var aggregator = new StatisticsAggregator();
    var reports = extractors.Select(e => aggregator.Aggregate(entries, e));

    // Format results to console
    var formatter = new ConsoleReportFormatter();
    formatter.Format(reports, Console.Out);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Error: File not found - {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}