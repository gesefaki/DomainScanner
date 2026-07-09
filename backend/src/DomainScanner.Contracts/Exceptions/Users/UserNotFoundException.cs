namespace DomainScanner.Contracts.Exceptions.Users;

/// <summary>
/// Exception when the user could not be found by this key.
/// </summary>
public class UserNotFoundException : Exception
{
    /// <inheritdoc />
    public UserNotFoundException(object key)
        : base($"User with key value {key} not found")
    {
    }
    
}