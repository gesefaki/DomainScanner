using System.Security.Cryptography;
using System.Text;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Contracts.Options;
using Microsoft.Extensions.Options;

namespace DomainScanner.Infrastructure.Auth.Authentication.LoginProtection;

public class HmacLoginAccountKeyProvider : ILoginAccountKeyProvider
{
    private readonly byte[] _secret;

    public HmacLoginAccountKeyProvider(IOptions<LoginAccountKeyOptions> options)
    {
        try
        {
            _secret = Convert.FromBase64String(
                options.Value.HmacSecret);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "Login account HMAC secret must be Base64.",
                exception);
        }

        if (_secret.Length < 32)
        {
            throw new InvalidOperationException(
                "Login account HMAC secret must contain at least 32 bytes.");
        }
    }

    /// <inheritdoc />
    public string Create(string normalizedEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);

        var emailBytes =
            Encoding.UTF8.GetBytes(normalizedEmail);

        var hash = HMACSHA256.HashData(
            _secret,
            emailBytes);

        return $"auth:login:v1:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}