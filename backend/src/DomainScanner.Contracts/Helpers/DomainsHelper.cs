using DomainScanner.Domain.Entities;

namespace DomainScanner.Contracts.Helpers;

/// <summary>
/// Provides helper methods for domain-related operations.
/// </summary>
public static class DomainsHelper
{
    /// <summary>
    /// Converts a DomainEntity adress to a <see cref="Uri"/> object.
    /// </summary>
    /// <param name="entity">The DomainEntity containing the address to convert.</param>
    /// <returns><see cref="Uri"/> object or <c>null</c> if address cannot be parsed.</returns>
    public static Uri? AddressToUri(DomainEntity entity)
    {
        try
        {
            Uri.TryCreate(entity.Address, UriKind.Absolute, out var uri);
            return uri;
        }
        catch (UriFormatException)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}