namespace DomainScanner.Domain.Entities;

public class User
{
    // Main Properties
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<DomainEntity> Domains { get; set; } = new List<DomainEntity>();
    public virtual ICollection<Ip> Ips { get; set; } = new List<Ip>();
}