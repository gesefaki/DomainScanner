namespace DomainScanner.Application.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string errors) : base(errors) {}
}