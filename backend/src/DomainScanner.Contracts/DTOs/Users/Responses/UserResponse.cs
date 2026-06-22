using DomainScanner.Contracts.DTOs.Domains;

namespace DomainScanner.Contracts.DTOs.Users;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DomainResponse[] Domains);