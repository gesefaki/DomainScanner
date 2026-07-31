namespace DomainScanner.Contracts.Options.Auth;

public static class AuthCookieOptions
{
    public const string Session = "__Host-domain-scanner-session";
    public const string Antiforgery = "__Host-domain-scanner-csrf";
}