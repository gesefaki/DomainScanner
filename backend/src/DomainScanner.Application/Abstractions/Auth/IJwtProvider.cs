using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Auth;

/// <summary>
/// Defines the contract for generating JWT for user authentication.
/// </summary>
public interface IJwtProvider
{
    /// <summary>
    /// Generates a JWT for the specified user.
    /// </summary>
    /// <param name="user">The user containing identity information for token generation.</param>
    /// <returns>A string representating the JWT for the authenticated user.</returns>
    string GenerateToken(User user);
}