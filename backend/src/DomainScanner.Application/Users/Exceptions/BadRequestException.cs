namespace DomainScanner.Application.Users.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(object key) : base($"Bad Request: user with ID {key} doesn't exist") {}
}