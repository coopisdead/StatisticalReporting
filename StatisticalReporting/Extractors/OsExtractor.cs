using StatisticalReporting.Interfaces;
using StatisticalReporting.Models;
using StatisticalReporting.Services;

namespace StatisticalReporting.Extractors;

public class OsExtractor : IDimensionExtractor
{
    private readonly UserAgentService _uaService;

    public OsExtractor(UserAgentService uaService)
    {
        _uaService = uaService;
    }

    public string DimensionName => "OS";

    public string Extract(LogEntry entry)
    {
        var result = _uaService.Parse(entry.UserAgent);
        var os = result.OS.Family;

        return string.IsNullOrWhiteSpace(os) || os == "Other" ? "Unknown" : os;
    }
}