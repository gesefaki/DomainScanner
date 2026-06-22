using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

public class User : BaseEntity
{
    // Main Properties
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual ICollection<DomainEntity> Domains { get; set; } = new List<DomainEntity>();
}