using MaxMind.GeoIP2;
using StatisticalReporting.Interfaces;
using StatisticalReporting.Models;

namespace StatisticalReporting.Extractors;

public class CountryExtractor : IDimensionExtractor, IDisposable
{
    private readonly DatabaseReader _reader;
    private readonly Dictionary<string, string> _cache = new();

    public CountryExtractor(string databasePath)
    {
        _reader = new DatabaseReader(databasePath);
    }

    public string DimensionName => "Country";

    public string Extract(LogEntry entry)
    {
        if (_cache.TryGetValue(entry.IpAddress, out var cached))
            return cached;

        string country;

        try
        {
            var response = _reader.Country(entry.IpAddress);
            country = string.IsNullOrWhiteSpace(response.Country.Name) ? "Unknown" : response.Country.Name;
        }
        catch
        {
            country = "Unknown";
        }

        _cache[entry.IpAddress] = country;
        return country;
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}