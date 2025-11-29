using DomainScanner.Core.DTO;
using DomainScanner.Core.Models;

namespace DomainScanner.Core.Mappers;

public static class DomainMapper
{
    // Из Domain в Response
    public static ResponseDomainDto ToResponse(this Domain domain)
    {
        ArgumentNullException.ThrowIfNull(domain, nameof(domain));
        return new ResponseDomainDto(domain.Id, domain.Name, domain.IsAvailable);
    }

    // Из Domain в Create
    public static CreateDomainDto ToCreate(this Domain domain)
    {
        ArgumentNullException.ThrowIfNull(domain, nameof(domain));
        return new CreateDomainDto(domain.Name);
    }
    
    // Из Domain в Update
    public static UpdateDomainDto ToUpdate(this Domain domain)
    {
        ArgumentNullException.ThrowIfNull(domain, nameof(domain));
        return new UpdateDomainDto(domain.Name, domain.IsAvailable);
    }

    // Из Response в Domain
    public static Domain ToDomain(this ResponseDomainDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));
        return new Domain
        {
            Id = dto.Id,
            Name = dto.Name,
            IsAvailable = dto.IsAvailable
        };
    }

    // Из Create в Domain
    public static Domain ToDomain(this CreateDomainDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));
        return new Domain
        {
            Name = dto.Name,
        };
    }
    
    // Из Update в Domain
    public static Domain ToDomain(this UpdateDomainDto dto, int id)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));
        return new Domain
        {
            Id = id,
            Name = dto.Name,
            IsAvailable = dto.IsAvailable
        };
    }
    
}