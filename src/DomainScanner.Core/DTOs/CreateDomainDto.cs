using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Api.DTOs;

public sealed class CreateDomainDto
{
    public int Id { get; init; }
    
    [Required, MaxLength(256)]
    public required string Name { get; init; }

    public CreateDomainDto(int id, string name)
    {
        Id = id;
        Name = name;
    }
}