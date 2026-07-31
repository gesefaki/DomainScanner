namespace DomainScanner.Application.Abstractions.Auth;

/// <summary>
/// Provides information about the user associated with the current request.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get;  }
    
    /// <summary>
    /// Gets the identifier of the current authenticated user.
    /// </summary>
    Guid Id { get; }
}