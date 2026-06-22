using FluentValidation;

namespace DomainScanner.Application.Helpers;

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
            throw new ValidationException("Uri is not valid: " + ex);
        }
    }
}