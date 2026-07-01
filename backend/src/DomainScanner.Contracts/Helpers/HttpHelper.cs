using DomainScanner.Contracts.Exceptions.Domains;

namespace DomainScanner.Contracts.Helpers;

public static class HttpHelper
{
    public static Uri AddressToUri(this string address)
    {
        try
        {
            return new Uri(address);
        }
        catch (Exception ex)
        {
            throw new DomainUriValidationException("Uri is not valid: " + ex);
        }
    }
}