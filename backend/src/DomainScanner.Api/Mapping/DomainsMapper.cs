using DomainScanner.Api.DTOs.Domains;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Api.Mapping;

public static class DomainsMapper
{
    public static DomainResponseDto DomainToDomainResponseDto(DomainEntity domain)
    {
        return new DomainResponseDto(
            domain.Id,
            domain.Address,
            domain.IsAvailable,
            domain.UserId
        );
    }
    public static DomainEntity CreateDomainDtoToUser(CreateDomainDto dto)
    {
        return new DomainEntity
        {
            Id = Guid.NewGuid(),
            Address = dto.Address,
            IsAvailable = null,
            UpdatedAt = null,
            CheckResults = [],
            UserId = dto.UserId
        };
    }
}