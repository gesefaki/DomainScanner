namespace DomainScanner.Contracts.Exceptions.Domains;

/// <summary>
/// Exception when domain address is invalid for convert to URI. Used primarily for HTTP requests
/// </summary>
public class DomainUriValidationException : Exception
{
    /// <inheritdoc />
    public DomainUriValidationException(string address) :  base($"Address {address} is invalid") { }
}