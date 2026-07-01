using System.Net;
using System.Net.Sockets;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Contracts.Helpers;

public static class IpsHelper
{
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