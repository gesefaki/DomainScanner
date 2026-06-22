using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

public class DomainCheckResult : BaseEntity
{
    // Main properties
    public string Address { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    
    // Navigation properties
    public Guid DomainId { get; set; } // FK
    public DomainEntity? DomainEntity { get; set; } // Navigation
    
}