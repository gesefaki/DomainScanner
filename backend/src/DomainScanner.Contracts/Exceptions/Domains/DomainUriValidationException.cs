namespace DomainScanner.Application.Exceptions;

public class UriValidationException : Exception
{
    public UriValidationException(string address) :  base($"Address {address} is invalid") { }
}