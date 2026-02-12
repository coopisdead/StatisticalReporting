using System.Text.RegularExpressions;
using StatisticalReporting.Interfaces;
using StatisticalReporting.Models;

namespace StatisticalReporting.Parsers;

public class ApacheLogParser : ILogParser
{
    private static readonly Regex LogPattern = new Regex(
        @"^(?<ip>\S+)\s+\S+\s+\S+\s+\[.*?\]\s+""(?<method>\S+)\s+\S+\s+\S+""\s+(?<status>\d{3})\s+\S+\s+"".*?""\s+""(?<useragent>.*?)""$",
        RegexOptions.Compiled
    );

    public int SkippedLines { get; private set; }

    public IEnumerable<LogEntry> Parse(string filePath)
    {
        SkippedLines = 0;

        foreach (var line in File.ReadLines(filePath))
        {
            var match = LogPattern.Match(line);

            if (!match.Success)
            {
                SkippedLines++;
                continue;
            }

            if (!int.TryParse(match.Groups["status"].Value, out var statusCode))
                statusCode = 0;

            yield return new LogEntry(
                IpAddress: match.Groups["ip"].Value,
                UserAgent: match.Groups["useragent"].Value,
                StatusCode: statusCode,
                RequestMethod: match.Groups["method"].Value
            );
        }
    }
}