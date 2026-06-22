using DomainScanner.Application.Abstractions;
using DomainScanner.Application.Abstractions.Auth;

namespace DomainScanner.Infrastructure.Auth.Hashing;

public class PasswordHasher : IPasswordHasher
{
    public string Generate(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    
    public bool Verify(string password, string hashedPassword) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
}