using DomainScanner.Domain.Entities;

namespace DomainScanner.Contracts.Helpers;

public static class DomainsHelper
{
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