using DomainScanner.Domain.Entities;

namespace DomainScanner.Api.DTOs.Users;

public record UserResponseDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    List<DomainEntity> Domains,
    List<DomainCheckResult> CheckResults);