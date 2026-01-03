namespace DomainScanner.Application.Exceptions;

public class UnableToExecuteException : Exception
{
    public UnableToExecuteException(object obj, object key, string value)
        : base($"Unable to execute operation with object {obj} with id {key} and parameter {value}") { } 
}