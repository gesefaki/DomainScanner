using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

public class DomainEntity : BaseEntity
{
    // Main Properties
    public string Address { get; set; } = string.Empty;
    
    // Navigation Properties
    public Guid UserId { get; set; } // FK
    public User? User { get; set; } // Navigation Property
    
    public virtual ICollection<DomainCheckResult> CheckResults { get; set; } =  new List<DomainCheckResult>();
    
}