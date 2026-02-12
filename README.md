# Statistical Reporting Module - Design Document

## Overview

A .NET 9 console application that reads an Apache web server log file (10,000 lines) and produces a statistical report showing the percentage breakdown of requests by **Country**, **OS**, and **Browser**.

## High-Level Architecture

```
                                            ┌──────────────────┐
                                            │  GeoLite2 DB     │
                                            │  (.mmdb file)    │
                                            └────────┬─────────┘
                                                     │
┌─────────────┐     ┌──────────────┐     ┌──────────────────────┐     ┌────────────────┐     ┌──────────────────┐
│  Log File   │────▶│  ILogParser  │───▶│ IDimensionExtractors │───▶│   Aggregator   │────▶│ IReportFormatter │
│ (raw text)  │     │ (interface)  │     │  (Country/OS/etc..)  │     │  (count + %)   │     │   (interface)    │
└─────────────┘     └──────────────┘     └──────────────────────┘     └────────────────┘     └──────────────────┘
```

**Flow:**
1. `LogParser` reads each line, extracts relevant fields into a `LogEntry` object
2. Each `IDimensionExtractor` processes every `LogEntry` and returns a label (e.g., "Chrome", "United States")
3. `StatisticsAggregator` groups and counts labels, calculates percentages
4. `IReportFormatter` outputs the results

## Key Abstractions

### LogEntry (Record)
A simple data object holding the parsed fields from a single log line.

```csharp
public record LogEntry(string IpAddress, string UserAgent, int StatusCode, string RequestMethod);
```

### ILogParser
Reads the log file and produces `LogEntry` objects.

```csharp
public interface ILogParser
{
    IEnumerable<LogEntry> Parse(string filePath);
}
```

**Implementation:** `ApacheLogParser` — uses a regex to extract fields from the standard Apache Combined Log Format.

### IDimensionExtractor (Core Extensibility Point)
Each dimension (Country, OS, Browser, etc..) implements this interface. This is the **Strategy Pattern** — every dimension follows the same contract, making it trivial to add new ones.

```csharp
public interface IDimensionExtractor
{
    string DimensionName { get; }
    string Extract(LogEntry entry);
}
```

**Implementations:**
- `CountryExtractor` — uses MaxMind GeoLite2 database to resolve IP → Country
- `OsExtractor` — uses UAParser library to parse User-Agent → OS family
- `BrowserExtractor` — uses UAParser library to parse User-Agent → Browser family

### StatisticsAggregator
Takes a collection of `LogEntry` objects and an `IDimensionExtractor`, produces a sorted percentage breakdown.

```csharp
public class StatisticsAggregator
{
    public DimensionReport Aggregate(IEnumerable<LogEntry> entries, IDimensionExtractor extractor);
}
```

Returns a `DimensionReport` containing the dimension name and a list of `(label, percentage)` pairs sorted descending by percentage.

### IReportFormatter
Formats the final report for output.

```csharp
public interface IReportFormatter
{
    void Format(IEnumerable<DimensionReport> reports, TextWriter output);
}
```

**Implementation:** `ConsoleReportFormatter` — prints to console in the required format (label + percentage).

## Extensibility

### Adding a New Dimension
To add a new dimension (e.g., "HTTP Method" or "Status Code"):

1. Create a new class implementing `IDimensionExtractor`
2. Register it in the extractor list in `Program.cs`
3. No changes to any existing code

```csharp
// Example: adding HTTP Method as a new dimension
public class HttpMethodExtractor : IDimensionExtractor
{
    public string DimensionName => "HTTP Method";
    public string Extract(LogEntry entry) => entry.RequestMethod;
}
```

This follows the **Open/Closed Principle** — the system is open for new extractors but no existing code changes.

### Adding a New Output Format
To output as JSON or CSV instead of console text:

1. Create a new class implementing `IReportFormatter` (e.g., `JsonReportFormatter`)
2. Swap it in at the composition root

### Changing the Log Format
To support a different log format (e.g., Nginx):

1. Create a new class implementing `ILogParser` (e.g., `NginxLogParser`)
2. Swap it in at the composition root

## External Dependencies & Data

### GeoLite2 Country Database
The `CountryExtractor` relies on MaxMind's GeoLite2 Country database (a local `.mmdb` file) for IP-to-country resolution. This is a **local file lookup**, not a web API call — so there are no rate limits or network dependencies at runtime.

The database path is passed into `CountryExtractor` via its constructor, keeping it configurable and testable:

```csharp
public class CountryExtractor : IDimensionExtractor
{
    private readonly DatabaseReader _reader;

    public CountryExtractor(string databasePath)
    {
        _reader = new DatabaseReader(databasePath);
    }

    public string DimensionName => "Country";
    public string Extract(LogEntry entry)
    {
        try { return _reader.Country(entry.IpAddress).Country.Name ?? "Unknown"; }
        catch { return "Unknown"; }
    }
}
```

**Note:** The `.mmdb` file is not committed to the repo due to MaxMind's license terms. A README will document how to download it.

### UAParser
Both `OsExtractor` and `BrowserExtractor` use the `UAParser` NuGet package, which bundles its own regex definitions internally — no external files needed.

## Technology Choices

| Choice | Rationale |
|--------|-----------|
| **.NET 9 / C#** | Strongly typed, natural support for interfaces and DI, excellent for demonstrating design patterns |
| **MaxMind.GeoIP2** (NuGet) | Official library for GeoLite2 database lookups. Local database — no API rate limits |
| **UAParser** (NuGet) | Lightweight, well-maintained User-Agent parsing based on the ua-parser project |
| **Regex** | Standard approach for parsing Apache log format. Simple and reliable for a known format |

## Trade-offs and Assumptions

- **In-memory processing**: 10,000 lines fits easily in memory. For much larger files, we'd switch to streaming with batched processing.
- **Unknown values**: If an IP can't be resolved or a User-Agent can't be parsed, the entry is labeled `"Unknown"` rather than skipped — every request is counted.
- **Single-threaded**: No parallelism needed for 10K lines. For millions of lines, we could parallelize extraction using `Parallel.ForEach`.
- **No dependency injection container**: For a small console app, manual composition in `Program.cs` is cleaner than adding a DI framework. The interfaces still allow easy testing and swapping.