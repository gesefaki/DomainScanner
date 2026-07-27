namespace DomainScanner.Contracts.Exceptions.Common;

/// <summary>
/// Exception when request cant be authorized.
/// </summary>
public class NonAuthorizedException : Exception
{
    /// <inheritdoc />
    public NonAuthorizedException() : base("Cannot authorize.")
    {
        
    }
}