using DomainScanner.Api.DTOs.Users;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Api.Mapping;

public static class UsersMapper
{
    public static UserResponseDto UserToResponseUserDto(User user)
    {
        return new UserResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.IsActive,
            user.Domains.Select(DomainsMapper.DomainToDomainResponseDto).ToArray()
        );
    }
    
    public static User CreateUserDtoToUser(CreateUserDto dto)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true,
            Domains = []
        };
    }
}