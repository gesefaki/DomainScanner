using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using DomainScanner.Contracts.Exceptions.HTTP;

namespace DomainScanner.Infrastructure.Protocols.HTTP;

/// <summary>
/// A class that verifies the security of the HTTP endpoint being passed.
/// </summary>
public class PublicNetworkHttpHandler
{
    private static readonly HashSet<int> AllowedPorts = [80, 443];

    /// <summary>
    /// Create new HTTP socket instance with socket configuration and <see cref="ConnectToPublicAddressAsync"/> validation callback.
    /// </summary>
    /// <param name="certificateValidation"></param>
    /// <returns></returns>
    public static SocketsHttpHandler Create(
        RemoteCertificateValidationCallback? certificateValidation = null)
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,

            // System proxy couldn't change request route
            UseProxy = false,
            
            // TODO: add from the configuration
            ConnectTimeout = TimeSpan.FromSeconds(10),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),

            MaxConnectionsPerServer = 10,

            // Value in kilobytes
            MaxResponseHeadersLength = 32,

            ConnectCallback = ConnectToPublicAddressAsync
        };

        if (certificateValidation is not null)
        {
            handler.SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = certificateValidation
            };
        }

        return handler;
    }

    /// <summary>
    /// Validate URI format. It should be absolute, http/https scheme, not empty and has port. Otherwise, HTTP endpoint isn't valid.  
    /// </summary>
    /// <param name="uri">URI for validation.</param>
    /// <exception cref="UnsafeOutboundDestinationException">Throws when one or more validation rules have been violated.</exception>
    public static void ValidateUri(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
        {
            throw new UnsafeOutboundDestinationException();
        }

        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new UnsafeOutboundDestinationException();
        }

        if (string.IsNullOrWhiteSpace(uri.Host) ||
            !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new UnsafeOutboundDestinationException();
        }

        if (!AllowedPorts.Contains(uri.Port))
        {
            throw new UnsafeOutboundDestinationException();
        }
    }

    /// <summary>
    /// A callback that checks address is public.
    /// </summary>
    /// <param name="context">Socket HTTP context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Supported connection stream if address is valid, otherwise <c>throw</c>.</returns>
    /// <exception cref="UnsafeOutboundDestinationException">Throws when address isn't secure.</exception>
    /// <exception cref="HttpRequestException">Throws when connection can't be determined or IP hasn't public DNS hostname.</exception>
    private static async ValueTask<Stream> ConnectToPublicAddressAsync(
        SocketsHttpConnectionContext context,
        CancellationToken ct)
    {
        var endpoint = context.DnsEndPoint;

        if (!AllowedPorts.Contains(endpoint.Port))
        {
            throw new UnsafeOutboundDestinationException();
        }

        IPAddress[] addresses;

        if (IPAddress.TryParse(endpoint.Host, out var literalAddress))
        {
            addresses = [literalAddress];
        }
        else
        {
            addresses = await Dns.GetHostAddressesAsync(
                endpoint.Host,
                ct);
        }

        if (addresses.Length == 0)
        {
            throw new HttpRequestException(
                "The destination hostname has no IP addresses.");
        }
        
        // If any DNS address is not-public, reject the all hostname.
        if (addresses.Any(address => !IsPublicAddress(address)))
        {
            throw new UnsafeOutboundDestinationException();
        }

        Exception? lastError = null;

        foreach (var address in addresses)
        {
            var socket = new Socket(
                address.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp)
            {
                NoDelay = true
            };

            try
            {
                // Connect with IP, not hostname. It's close the DNS rebinding.
                await socket.ConnectAsync(
                    new IPEndPoint(address, endpoint.Port),
                    ct);

                return new NetworkStream(
                    socket,
                    ownsSocket: true);
            }
            catch (OperationCanceledException)
            {
                socket.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                socket.Dispose();
                lastError = ex;
            }
        }

        throw new HttpRequestException(
            "Unable to connect to the destination.",
            lastError);
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address) ||
            address.Equals(IPAddress.Any) ||
            address.Equals(IPAddress.None) ||
            address.Equals(IPAddress.IPv6Any) ||
            address.Equals(IPAddress.IPv6None))
        {
            return false;
        }

        var bytes = address.GetAddressBytes();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return IsPublicIpv4(bytes);
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return IsPublicIpv6(bytes);
        }
        
        return false;
    }

    private static bool IsPublicIpv4(byte[] ip)
    {
        // 0.0.0.0/8
        if (ip[0] == 0)
            return false;

        // 10.0.0.0/8
        if (ip[0] == 10)
            return false;

        // 100.64.0.0/10 — CGNAT
        if (ip[0] == 100 && ip[1] is >= 64 and <= 127)
            return false;

        // 127.0.0.0/8 — loopback
        if (ip[0] == 127)
            return false;

        // 169.254.0.0/16 — link-local, including metadata IP
        if (ip[0] == 169 && ip[1] == 254)
            return false;

        // 172.16.0.0/12
        if (ip[0] == 172 && ip[1] is >= 16 and <= 31)
            return false;

        // 192.0.0.0/24 — special purpose
        if (ip[0] == 192 && ip[1] == 0 && ip[2] == 0)
            return false;

        // 192.0.2.0/24 — documentation
        if (ip[0] == 192 && ip[1] == 0 && ip[2] == 2)
            return false;

        // 192.88.99.0/24 — reserved/relay
        if (ip[0] == 192 && ip[1] == 88 && ip[2] == 99)
            return false;

        // 192.168.0.0/16
        if (ip[0] == 192 && ip[1] == 168)
            return false;

        // 198.18.0.0/15 — benchmarking
        if (ip[0] == 198 && ip[1] is 18 or 19)
            return false;

        // 198.51.100.0/24 — documentation
        if (ip[0] == 198 && ip[1] == 51 && ip[2] == 100)
            return false;

        // 203.0.113.0/24 — documentation
        if (ip[0] == 203 && ip[1] == 0 && ip[2] == 113)
            return false;

        // 224.0.0.0/4 multicast and 240.0.0.0/4 reserved
        if (ip[0] >= 224)
            return false;

        return true;
    }
    
    private static bool IsPublicIpv6(byte[] ip)
    {
        // ::/96 — unspecified, IPv4-compatible and other special-use.
        if (ip.Take(12).All(value => value == 0))
            return false;

        // fc00::/7 — unique local
        if ((ip[0] & 0xFE) == 0xFC)
            return false;

        // fe80::/10 link-local and fec0::/10 deprecated site-local.
        if (ip[0] == 0xFE && ip[1] >= 0x80)
            return false;

        // ff00::/8 — multicast
        if (ip[0] == 0xFF)
            return false;

        // 2001:db8::/32 — documentation
        if (ip[0] == 0x20 &&
            ip[1] == 0x01 &&
            ip[2] == 0x0D &&
            ip[3] == 0xB8)
        {
            return false;
        }

        // 2002::/16 — 6to4.
        // For current development stage is not supported.
        if (ip[0] == 0x20 && ip[1] == 0x02)
            return false;

        return true;
    }
}