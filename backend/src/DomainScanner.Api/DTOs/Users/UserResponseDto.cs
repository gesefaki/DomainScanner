using DomainScanner.Api.DTOs.Domains;

namespace DomainScanner.Api.DTOs.Users;

public record UserResponseDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DomainResponseDto[] Domains);