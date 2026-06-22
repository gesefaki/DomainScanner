namespace DomainScanner.Contracts.Exceptions.Domains;

public class DomainInvalidAddressFormatException : Exception
{
    public DomainInvalidAddressFormatException(string name) : base($"Address {name} is invalid")
    {
    }
}