namespace DomainScanner.DataAccess.Postgre.Models;

public class Domain
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool? IsAvailable { get; set; } = null;
}