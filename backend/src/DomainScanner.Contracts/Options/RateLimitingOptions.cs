namespace DomainScanner.Contracts.Options;

public static class RateLimitingOptions
{
    public const string Read = "read";
    public const string Write = "write";
    public const string Auth = "auth";
    public const string Scan = "scan";
    public const string Login = "login";
}