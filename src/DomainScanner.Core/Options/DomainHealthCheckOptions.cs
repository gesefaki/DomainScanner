namespace DomainScanner.Core.Options;

public class DomainHealthCheckOptions
{
    public const string SectionName = "DomainHealthCheck";

    public string HttpClientName { get; set; } = "scanner";
}