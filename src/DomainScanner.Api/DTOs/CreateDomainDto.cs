using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Api.DTOs;

public class CreateDomainDto
{
    public int Id { get; set; }
    
    [Required, MaxLength(256)]
    public required string Name { get; init; }
}