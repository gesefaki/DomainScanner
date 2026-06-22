namespace DomainScanner.Application.Exceptions;

public class UserInvalidCredentialsException : Exception
{
    public UserInvalidCredentialsException() : base("Invalid email or password")
    {
    }
}