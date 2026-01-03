using DomainScanner.Api.DTOs.Domains;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Api.DTOs.Users;

public record UserResponseDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DomainResponseDto[] Domains);