namespace DomainScanner.Contracts.Exceptions.Users;

public class UserInvalidCredentialsException : Exception
{
    public UserInvalidCredentialsException() : base("Invalid email or password")
    {
    }
}