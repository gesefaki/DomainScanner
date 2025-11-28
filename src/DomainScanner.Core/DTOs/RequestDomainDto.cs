using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Api.DTOs;

public sealed class RequestDomainDto
{
    public int Id { get; init; }
    [Required, MaxLength(256)]
    public required string Name { get; init; }
    
    public bool? IsAvailable { get; init; }

    public RequestDomainDto(int id, string name, bool? isAvailable)
    {
        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }
    
}