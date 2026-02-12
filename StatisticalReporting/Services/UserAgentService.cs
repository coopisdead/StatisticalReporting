using UAParser;

namespace StatisticalReporting.Services;

public class UserAgentService
{
    private readonly Parser _parser;
    private readonly Dictionary<string, ClientInfo> _cache = new();

    public UserAgentService()
    {
        _parser = Parser.GetDefault();
    }

    public ClientInfo Parse(string userAgent)
    {
        if (_cache.TryGetValue(userAgent, out var cached))
            return cached;

        var result = _parser.Parse(userAgent);
        _cache[userAgent] = result;
        return result;
    }
}