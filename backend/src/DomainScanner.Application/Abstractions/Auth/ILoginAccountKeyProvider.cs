namespace DomainScanner.Application.Abstractions.Auth;

/// <summary>
/// Defines the contract for generating unique user keys.
/// </summary>
public interface ILoginAccountKeyProvider
{
    /// <summary>
    /// Generates a unique key based on the provided email address.
    /// </summary>
    /// <param name="normalizedEmail">Normalized email to create from.</param>
    /// <returns>Unique key of email.</returns>
    string Create(string normalizedEmail);
}