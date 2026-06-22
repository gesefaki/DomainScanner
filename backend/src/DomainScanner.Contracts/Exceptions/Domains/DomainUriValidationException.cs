namespace DomainScanner.Contracts.Exceptions.Domains;

public class DomainUriValidationException : Exception
{
    public DomainUriValidationException(string address) :  base($"Address {address} is invalid") { }
}