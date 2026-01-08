namespace DomainScanner.Application.Exceptions;

public class InvalidAddressFormatException : Exception
{
    public InvalidAddressFormatException(string name) : base($"Address {name} is invalid")
    {
    }
}