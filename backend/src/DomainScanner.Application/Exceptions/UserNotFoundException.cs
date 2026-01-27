namespace DomainScanner.Application.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(object key)
        : base($"User with key value {key} not found")
    {
    }
    
}