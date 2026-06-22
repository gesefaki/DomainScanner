using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

public class Ip : BaseEntity
{
    // Main Properties
    public string Address { get; set; } = string.Empty;
    public ushort? Port { get; set; }
    
    // Navigation Properties
    // User
    public Guid UserId { get; set; } // FK
    public User? User { get; set; }
    
    // Domain
    public Guid DomainId { get; set; } // FK
    public DomainEntity? Domain { get; set; }
}