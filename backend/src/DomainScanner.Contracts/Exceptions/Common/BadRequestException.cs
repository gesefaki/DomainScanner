namespace DomainScanner.Contracts.Exceptions.Common;

/// <summary>
/// Exception when a request contains invalid data.
/// </summary>
public class BadRequestException : Exception
{
    /// <inheritdoc />
    public BadRequestException(string errors) : base(errors) {}
}