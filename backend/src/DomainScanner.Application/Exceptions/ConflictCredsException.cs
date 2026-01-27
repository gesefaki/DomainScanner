namespace DomainScanner.Application.Exceptions;

public class ConflictCredsException : Exception
{
    public ConflictCredsException() : base("User with that username or email already exists.")
    {
    } 
}