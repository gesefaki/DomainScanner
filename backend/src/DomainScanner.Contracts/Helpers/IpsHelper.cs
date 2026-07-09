using System.Net;
using System.Net.Sockets;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Contracts.Helpers;

/// <summary>
/// Provides helper methods for IP address validation and manipulation.
/// </summary>
public static class IpsHelper
{
    /// <summary>
    /// Validates and parses an IP address from an <see cref="Ip"/> entity.
    /// </summary>
    /// <param name="ip">The IP entity containing the address to validate.</param>
    /// <returns><see cref="IPAddress"/> object or <c>null</c> if address is cannot be parsed or invalid in accordance with the application's logic.</returns>
    public static IPAddress? ValidateAndGetIp(Ip ip)
    {
        bool valid = IPAddress.TryParse(ip.Address, out var result);
        if (!valid)
            return null;

        if (result!.AddressFamily == AddressFamily.InterNetwork)
            return null;

        return result;
    }
}