using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions;

public interface IJwtProvider
{
    string GenerateToken(User user);
}