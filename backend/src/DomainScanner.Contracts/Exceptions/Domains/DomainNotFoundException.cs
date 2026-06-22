namespace DomainScanner.Contracts.Exceptions.Domains;

public class DomainNotFoundException : Exception
{
    public DomainNotFoundException(object key)
        : base($"Domain with key {key} could not be found.")
    {}
}