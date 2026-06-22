namespace DomainScanner.Contracts.Exceptions.Common;

public class BadRequestException : Exception
{
    public BadRequestException(string errors) : base(errors) {}
}