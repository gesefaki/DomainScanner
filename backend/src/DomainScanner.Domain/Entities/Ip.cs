using DomainScanner.Domain.Common;

namespace DomainScanner.Domain.Entities;

/// <summary>
/// Represents an IP address entity that is monitored for scanning and tracking purposes. Inherits from <see cref="BaseEntity"/>. 
/// </summary>
public class Ip : BaseEntity
{
    /// <summary>
    /// IP addres in standard notation format.
    /// </summary>
    /// <value>
    /// The IPv4 address as a string. Examples: "192.168.1.1".
    /// </value>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Optional port number associated with this IP address. 
    /// </summary>
    /// <value>
    /// A nullable ushort representing the port number (1-65535), or null if no specific port is tracked.
    /// </value>
    public ushort? Port { get; set; }
    
    /// <summary>
    /// Unique identifier of the User who owns or manages this IP address.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The navigation property to the associated User.
    /// </summary>
    public User? User { get; set; }
    
    /// <summary>
    /// Unique identifier of the DomainEntity to which this IP address is linked
    /// </summary>
    public Guid DomainId { get; set; }

    /// <summary>
    /// The navigation property to the associated DomainEntity.
    /// </summary>
    public DomainEntity? Domain { get; set; }
}