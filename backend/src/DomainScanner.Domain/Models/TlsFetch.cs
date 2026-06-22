using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace DomainScanner.Domain.ValueObjects;

public class TlsFetch
{
    public string? Message { get; set; }
    public string? Certificate { get; set; }
    public string? Chain { get; set; }
    public bool? SslPolicyErrors { get; set; }
}