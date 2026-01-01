namespace DomainScanner.Application.Users.Exceptions;

public class UnableToExecuteException : Exception
{
    public UnableToExecuteException(object obj, object key, object value)
        : base($"Unable to execute operation with object {obj} with id {key} and parameter {value}") { } 
}