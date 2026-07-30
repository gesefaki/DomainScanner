namespace DomainScanner.Contracts.Exceptions.Common;

public class LoginProtectionUnavailableException : Exception
{
    public LoginProtectionUnavailableException() 
        : base("Login protection service is unavailable. Check Redis.")
    {
        
    }
}