namespace DomainScanner.Application.Abstractions.Auth;

/// <summary>
/// Defines a general contract for normalizing email addresses, regardless of the service using it.
/// </summary>
public interface IEmailNormalizer
{
    /// <summary>
    /// Normalizes the address to its standard form.
    /// </summary>
    /// <param name="email">Email to normalize.</param>
    /// <returns>Normalized email.</returns>
    string Normalize(string email);
}