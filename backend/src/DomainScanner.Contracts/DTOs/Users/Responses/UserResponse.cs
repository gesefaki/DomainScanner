using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Contracts.DTOs.Users.Responses;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DomainResponse[] Domains);