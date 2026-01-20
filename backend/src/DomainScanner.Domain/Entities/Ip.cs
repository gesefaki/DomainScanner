using System.Net;
using System.Net.Sockets;

namespace DomainScanner.Domain.Entities;

public class Ip
{
    // Main Properties
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Address { get; set; } = string.Empty;
    public ushort? Port { get; set; }
    public bool? IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    // User
    public Guid UserId { get; set; } // FK
    public User? User { get; set; }
    
    // Domain
    public Guid DomainId { get; set; } // FK
    public DomainEntity? Domain { get; set; }

    public IPAddress? ValidateAndGetIp()
    {
        bool valid = IPAddress.TryParse(this.Address, out var ip);
        if (!valid)
            return null;

        if (ip!.AddressFamily == AddressFamily.InterNetwork)
            return null;

        return ip;
    }
}