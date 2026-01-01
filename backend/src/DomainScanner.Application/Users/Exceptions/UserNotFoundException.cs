namespace DomainScanner.Application.Users.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string name, object key)
        : base($"User {name} with id {key} not found")
    {
    }
    
}