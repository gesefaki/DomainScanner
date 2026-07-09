namespace DomainScanner.Contracts.Exceptions.Domains;

/// <summary>
/// Exception when the domain could not be found by this key.
/// </summary>
public class DomainNotFoundException : Exception
{
    /// <inheritdoc />
    public DomainNotFoundException(object key)
        : base($"Domain with key {key} could not be found.")
    {}
}