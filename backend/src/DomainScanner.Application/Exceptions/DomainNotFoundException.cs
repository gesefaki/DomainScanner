namespace DomainScanner.Application.Exceptions;

public sealed class DomainNotFoundException : Exception
{
    public DomainNotFoundException(object key)
        : base($"Domain with key {key} could not be found.")
    {}
}