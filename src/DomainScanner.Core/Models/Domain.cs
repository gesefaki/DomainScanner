namespace DomainScanner.Core.Models;

public class Domain
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool? IsAvailable { get; set; } = null;
    
}