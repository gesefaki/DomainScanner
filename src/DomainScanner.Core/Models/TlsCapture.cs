using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace DomainScanner.Core.Models;

public class TlsCapture
{
    public X509Certificate2? ServerCertificate { get; set; }
    public X509Chain? CertificateChain { get; set; }
    public SslPolicyErrors SslPolicyErrors { get; set; }
}