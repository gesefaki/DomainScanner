using System.Net.NetworkInformation;

namespace DomainScanner.Infrastructure.Protocols.ICMP;

public static class IcmpService
{
    public static async Task<long> GetPingTimeAsync(string address, CancellationToken ct)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(address, 5000);

            if (reply.Status == IPStatus.Success)
            {
                return reply.RoundtripTime;
            }

            return -1;
        }
        catch(PingException)
        {
            return -1;
        }
    }
}