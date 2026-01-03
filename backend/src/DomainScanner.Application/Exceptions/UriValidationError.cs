namespace DomainScanner.Application.Exceptions;

public class UriValidationError : Exception
{
    public UriValidationError(string address) :  base($"Address {address} is invalid") { }
}