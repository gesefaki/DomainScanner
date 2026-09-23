namespace DomainScanner.Contracts.Exceptions.Domains;

/// <summary>Raised when creating a domain would exceed the current user's ownership quota.</summary>
public sealed class DomainQuotaExceededException : Exception
{
    /// <summary>The maximum number of domains allowed per user.</summary>
    public int Limit { get; }

    /// <summary>Initializes an exception for the configured domain limit.</summary>
    /// <param name="limit">The configured maximum domain count.</param>
    public DomainQuotaExceededException(int limit)
        : base($"Domain quota of {limit} has been reached.")
    {
        Limit = limit;
    }
}
