namespace DomainScanner.Contracts.Exceptions.Common;

public class UnableToExecuteException : Exception
{
    public UnableToExecuteException(object obj, object key, string value)
        : base($"Unable to execute operation with object {obj} with id {key} and parameter {value}") { } 
}