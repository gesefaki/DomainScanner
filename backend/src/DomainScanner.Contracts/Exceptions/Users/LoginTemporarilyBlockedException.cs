namespace DomainScanner.Contracts.Exceptions.Users;

public class LoginTemporarilyBlockedException : Exception
{
    public TimeSpan RetryAfter { get; }

    public LoginTemporarilyBlockedException(TimeSpan retryAfter)
        : base($"Login attempt is blocked. Retry after {Math.Ceiling(retryAfter.TotalSeconds)} seconds.")
    {
        RetryAfter = retryAfter;
    }
}
