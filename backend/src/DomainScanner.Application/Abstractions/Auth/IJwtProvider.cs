using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Auth;

public interface IJwtProvider
{
    string GenerateToken(User user);
}