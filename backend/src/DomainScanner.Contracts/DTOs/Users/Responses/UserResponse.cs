using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Contracts.DTOs.Users.Responses;

/// <summary>
/// Public account data with protocol-neutral domain summaries.
/// </summary>
public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DomainResponse[] Domains);
