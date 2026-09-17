namespace DomainScanner.Contracts.Exceptions.HTTP;

public sealed class UnsafeOutboundDestinationException : HttpRequestException
{
    public UnsafeOutboundDestinationException()
        : base("Outbound destination is not permitted.")
    {
    }
}