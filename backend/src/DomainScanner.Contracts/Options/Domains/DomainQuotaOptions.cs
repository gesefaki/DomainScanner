namespace DomainScanner.Contracts.Options.Domains;

/// <summary>Limits the number of domains owned by one user.</summary>
public sealed class DomainQuotaOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "DomainQuota";

    /// <summary>The positive ownership limit, including paused and unavailable domains. Defaults to 50.</summary>
    public int MaxDomainsPerUser { get; init; } = 50;
}
