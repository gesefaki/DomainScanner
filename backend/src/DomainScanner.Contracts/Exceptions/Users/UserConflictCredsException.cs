namespace DomainScanner.Contracts.Exceptions.Users;

/// <summary>
/// Exception when a user registration attempt conflicts with existing creds.
/// </summary>
public class UserConflictCredsException : Exception
{
    /// <inheritdoc />
    public UserConflictCredsException() : base("User with that username or email already exists.")
    {
    } 
}