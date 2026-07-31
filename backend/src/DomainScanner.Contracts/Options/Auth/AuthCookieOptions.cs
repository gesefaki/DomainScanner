namespace DomainScanner.Contracts.Options.Auth;

/// <summary>
/// Contains names of authentication-related cookies.
/// </summary>
public static class AuthCookieOptions
{
    /// <summary>
    /// Session cookie name.
    /// </summary>
    public const string Session = "__Host-domain-scanner-session";

    /// <summary>
    /// Antiforgery cookie name.
    /// </summary>
    public const string Antiforgery = "__Host-domain-scanner-csrf";
}
