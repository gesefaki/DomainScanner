using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Api.DTOs;

public sealed class DomainDto
{
    public int Id { get; init; }
    [Required, MaxLength(256)]
    public required string Name { get; init; }
    
    public bool? IsAvailable { get; init; }
    
}