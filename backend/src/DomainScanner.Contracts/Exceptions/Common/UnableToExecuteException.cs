namespace DomainScanner.Contracts.Exceptions.Common;

/// <summary>
/// Exception when a command cannot be executed for any reason.
/// </summary>
public class UnableToExecuteException : Exception
{
    /// <inheritdoc />
    public UnableToExecuteException(object obj, object key, string value)
        : base($"Unable to execute operation with object {obj} with id {key} and parameter {value}") { } 
}