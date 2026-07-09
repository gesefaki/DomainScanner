namespace DomainScanner.Contracts.Options;

/// <summary>
/// Configuration settings for caching behavior in the application.
/// </summary>
public sealed class CacheSettings
{
    /// <summary>
    /// Default expiration time for cached items in minutes.
    /// </summary>
    /// <value>
    /// The number of minutes after which cached items should expire as <c>int</c>.
    /// Default is 2.
    /// </value>
    public int DefaultExpirationMinutes { get; set; } = 2;
}