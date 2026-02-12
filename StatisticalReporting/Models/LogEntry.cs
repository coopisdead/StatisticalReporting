namespace StatisticalReporting.Models;

public record LogEntry(string IpAddress, string UserAgent, int StatusCode, string RequestMethod);