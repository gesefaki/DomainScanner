namespace DomainScanner.Domain.Models;

public class TlsFetch
{
    public string? Message { get; set; }
    public string? Certificate { get; set; }
    public string? Chain { get; set; }
    public bool? SslPolicyErrors { get; set; }
}