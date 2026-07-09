namespace DomainScanner.Contracts.Exceptions.Users;

/// <summary>
/// Exception when user input invalid credentials data.
/// </summary>
public class UserInvalidCredentialsException : Exception
{
    /// <inheritdoc />
    public UserInvalidCredentialsException() : base("Invalid email or password")
    {
    }
}