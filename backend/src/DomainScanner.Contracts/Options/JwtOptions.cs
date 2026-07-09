namespace DomainScanner.Contracts.Options;

/// <summary>
/// Configuration options for JWT.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Symmetric key user for signing and validating JWT tokens.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in hours.
    /// </summary>
    /// <value>
    /// Number of hours until the token expires as <c>int</c>.
    /// </value>
    public int ExpiresHours { get; set; }
}