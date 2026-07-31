namespace DomainScanner.Contracts.Exceptions.Users;

/// <summary>
/// Represents a temporarily blocked login attempt.
/// </summary>
public class LoginTemporarilyBlockedException : Exception
{
    /// <summary>
    /// Gets the duration after which another login attempt can be made.
    /// </summary>
    public TimeSpan RetryAfter { get; }

    public LoginTemporarilyBlockedException(TimeSpan retryAfter)
        : base($"Login attempt is blocked. Retry after {Math.Ceiling(retryAfter.TotalSeconds)} seconds.")
    {
        RetryAfter = retryAfter;
    }
}
