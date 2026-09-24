using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;
/// <summary>
/// Represents a saved target whose checks may use different protocols.
/// </summary>
public class DomainEntity : BaseEntity
{
    /// <summary>
    /// The domain address as a string. Examples: "example.com", "subdomain.example.org".
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Whether scheduled monitoring is enabled. Defaults to true for new domains.</summary>
    /// <remarks>Independent of <see cref="BaseEntity.IsActive"/>, which stores measured availability.</remarks>
    public bool MonitoringEnabled { get; set; } = true;
    
    /// <summary>
    /// Unique identifier of the user who owns or manages this domain.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The navigation property to the associated user entity.
    /// </summary>
    public User? User { get; set; } // Navigation Property
    
    /// <summary>
    /// Retained results of protocol-specific checks. API domain responses expose
    /// only the most recent result summary; history has separate endpoints.
    /// </summary>
    public virtual ICollection<DomainCheckResult> CheckResults { get; set; } =  new List<DomainCheckResult>();
    
}
