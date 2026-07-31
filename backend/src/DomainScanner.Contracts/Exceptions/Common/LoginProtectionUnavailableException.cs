namespace DomainScanner.Contracts.Exceptions.Common;

/// <summary>
/// Represents an error caused by an unavailable login protection service.
/// </summary>
public class LoginProtectionUnavailableException : Exception
{
    public LoginProtectionUnavailableException() 
        : base("Login protection service is unavailable. Check Redis.")
    {
        
    }
}
