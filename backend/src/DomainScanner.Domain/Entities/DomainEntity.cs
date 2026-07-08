using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;
/// <summary>
///    Represents a domain entity that is monitored for scanning and tracking purposes. Inherits from <see cref="BaseEntity"/>
/// </summary>
public class DomainEntity : BaseEntity
{
    /// <summary>
    /// The domain address as a string. Examples: "example.com", "subdomain.example.org".
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique identifier of the user who owns or manages this domain.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The navigation property to the associated user entity.
    /// </summary>
    public User? User { get; set; } // Navigation Property
    
    /// <summary>
    /// The collection of domain check results associated with this domain.
    /// </summary>
    public virtual ICollection<DomainCheckResult> CheckResults { get; set; } =  new List<DomainCheckResult>();
    
}