namespace DomainScanner.Contracts.Exceptions.Users;

public class UserConflictCredsException : Exception
{
    public UserConflictCredsException() : base("User with that username or email already exists.")
    {
    } 
}