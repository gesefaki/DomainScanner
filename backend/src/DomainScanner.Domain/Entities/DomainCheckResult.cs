namespace DomainScanner.Domain.Entities;

public class DomainCheckResult
{
    // Main properties
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Address {  get; set; } = string.Empty;
    public int StatusCode {  get; set; }
    public bool IsAvailable {  get; set; }
    public DateTime CreatedAt {  get; set; }
    
    // Navigation properties
    public Guid UserId { get; set; } // FK
    public User? User { get; set; } // Navigation
    
    public Guid DomainId { get; set; } // FK
    public DomainEntity? DomainEntity { get; set; } // Navigation
}