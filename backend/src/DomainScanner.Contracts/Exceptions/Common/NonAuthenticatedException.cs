namespace DomainScanner.Contracts.Exceptions.Common;

/// <summary>
/// Exception when request cant be authorized.
/// </summary>
public class NonAuthenticatedException : Exception
{
    /// <inheritdoc />
    public NonAuthenticatedException() : base("Cannot authenticate.")
    {
        
    }
}