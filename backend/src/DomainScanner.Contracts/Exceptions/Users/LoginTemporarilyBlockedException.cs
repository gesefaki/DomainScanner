namespace DomainScanner.Contracts.Exceptions.Users;

public class LoginTemporarilyBlockedException : Exception
{
    public LoginTemporarilyBlockedException(TimeSpan retryAfter)
        : base($"Login attempt is blocked. RetryAfter ${retryAfter.Minutes}")
    {
        
    }
}