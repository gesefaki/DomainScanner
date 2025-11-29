namespace DomainScanner.Core.Options;

public class HttpClientOptions
{
    public const string SectionName = "HttpClientOptions";

    public int TimeoutSeconds { get; set; } = 15;
    public string UserAgent { get; set; } = "DomainScanner/1.0";
    public bool AllowAutoRedirect { get; set; } = true;
    public int MaxConnectionsPerServer { get; set; } = 50;
}