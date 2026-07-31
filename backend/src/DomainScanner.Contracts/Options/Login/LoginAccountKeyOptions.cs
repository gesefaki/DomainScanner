namespace DomainScanner.Contracts.Options.Login;

/// <summary>
/// Defines settings used to generate protected login account keys.
/// </summary>
public class LoginAccountKeyOptions
{
    /// <summary>
    /// Base64-encoded secret used for HMAC generation.
    /// </summary>
    public string HmacSecret { get; set; } = string.Empty;
}
