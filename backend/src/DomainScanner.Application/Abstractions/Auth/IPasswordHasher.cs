namespace DomainScanner.Application.Abstractions.Auth;

/// <summary>
/// Defines the contract for hashing and verifying password securely.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a secure hash for the provided password.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>A string containing the hashed password.</returns>
    string Generate(string password);

    /// <summary>
    /// Verifies that the provided password matches the stored hash.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hashedPassword">The stored hash to compare against.</param>
    /// <returns>True if the password matches the hash, otherwise false.</returns>
    bool Verify(string password, string hashedPassword);
}