using StatisticalReporting.Interfaces;
using StatisticalReporting.Models;
using StatisticalReporting.Services;

namespace StatisticalReporting.Extractors;

public class BrowserExtractor : IDimensionExtractor
{
    private readonly UserAgentService _uaService;

    public BrowserExtractor(UserAgentService uaService)
    {
        _uaService = uaService;
    }

    public string DimensionName => "Browser";

    public string Extract(LogEntry entry)
    {
        var result = _uaService.Parse(entry.UserAgent);
        var browser = result.UA.Family;

        return string.IsNullOrWhiteSpace(browser) || browser == "Other" ? "Unknown" : browser;
    }
}