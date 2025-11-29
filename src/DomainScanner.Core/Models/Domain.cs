namespace DomainScanner.Core.Models;

public class Domain
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public bool? IsAvailable { get; set; } = null;
    
}