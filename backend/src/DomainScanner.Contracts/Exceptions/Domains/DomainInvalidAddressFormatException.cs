namespace DomainScanner.Application.Exceptions;

public class DomaimInvalidAddressFormatException : Exception
{
    public DomaimInvalidAddressFormatException(string name) : base($"Address {name} is invalid")
    {
    }
}