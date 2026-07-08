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
    /// Indicates whether the object is accessible.
    /// </summary>
    public bool IsActive { get; set; }
}