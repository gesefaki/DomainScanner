using DomainScanner.Application.Abstractions.Auth;

namespace DomainScanner.Infrastructure.Auth.Hashing;

/// <summary>
/// Provides hash generation. Implements <see cref="IPasswordHasher"/> 
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string Generate(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        
    
    /// <inheritdoc />
    public bool Verify(string password, string hashedPassword) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
}