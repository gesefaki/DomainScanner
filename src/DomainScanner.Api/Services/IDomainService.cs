using DomainScanner.Api.DTOs;
using DomainScanner.Infrastructure.Models;

namespace DomainScanner.Api.Services;

public interface IDomainService
{
    public IEnumerable<DomainDto> GetAllDto();
    public DomainDto? GetDto(int id);
    public void Add(CreateDomainDto dto);
    public void Remove(int id);
    public void UpdateDto(UpdateDomainDto dto);
    public void UpdateHealthStatus(int id);
}