namespace DomainScanner.Domain.Common;

/// <summary>
/// The base class from which any entity stored in the database must inherit.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier of the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The entity's creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Most recent update to the entity, or null if it has never been updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Entity-specific active flag. For a domain or check it reflects the latest
    /// measured result; for a user it indicates whether the account is enabled.
    /// Scheduled monitoring is configured separately on the domain.
    /// </summary>
    public bool IsActive { get; set; }
}
