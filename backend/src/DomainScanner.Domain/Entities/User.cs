using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

/// <summary>
/// Represents a user account. Inherits from <see cref="BaseEntity"/> 
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Username of the user account.
    /// </summary>
    /// <value>
    /// Username as a string. Examples: "john_doe", "admin", "user123"
    /// </value>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for this user account.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Email for this user account.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Collection of DomainEntity owned or managed by this user.
    /// </summary>
    public virtual ICollection<DomainEntity> Domains { get; set; } = new List<DomainEntity>();
}