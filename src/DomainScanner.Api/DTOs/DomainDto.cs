using System.ComponentModel.DataAnnotations;

namespace DomainScanner.Api.DTOs;

public class DomainDto
{
    public int Id { get; set; }
    [Required, MaxLength(256)]
    public required string Name { get; set; }
    
    public bool? IsAvailable { get; set; }
}