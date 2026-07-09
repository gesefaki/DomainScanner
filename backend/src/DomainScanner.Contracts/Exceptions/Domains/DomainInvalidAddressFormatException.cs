namespace DomainScanner.Contracts.Exceptions.Domains;

/// <summary>
/// Exception when domains address in any format is invalid on creation.
/// </summary>
public class DomainInvalidAddressFormatException : Exception
{
    /// <inheritdoc />
    public DomainInvalidAddressFormatException(string name) : base($"Address {name} is invalid")
    {
    }
}