namespace DomainScanner.Application.Exceptions;

public sealed class DomainNotFoundException : Exception
{
    public DomainNotFoundException(string name, object key)
        : base($"Domain {name} with  key {key} could not be found.")
    {}
}