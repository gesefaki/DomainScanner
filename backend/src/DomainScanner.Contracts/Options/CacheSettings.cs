namespace DomainScanner.Contracts.Options;

public sealed class CacheSettings
{
    public int DefaultExpirationMinutes { get; set; } = 2;
}