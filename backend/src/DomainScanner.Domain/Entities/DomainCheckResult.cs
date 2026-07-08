using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

/// <summary>
/// An entity that stores the domain's response to a sent request. Associated with a specific DomainEntity that stores the request's address. Inherits from <see cref="BaseEntity"/> 
/// </summary>
public class DomainCheckResult : BaseEntity
{
    /// <summary>
    /// The address to which the request was sent.
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// The response code returned by the address.
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// The unique identifier of the DomainEntity to which the entity is linked.
    /// </summary>
    public Guid DomainId { get; set; }

    /// <summary>
    /// The navigation property to the associated DomainEntity.
    /// </summary>
    public DomainEntity? DomainEntity { get; set; }
    
}