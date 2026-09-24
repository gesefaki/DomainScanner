using DomainScanner.Infrastructure.Protocols.HTTP;

namespace DomainScanner.Infrastructure.UnitTests.Protocols;

/// <summary>Checks that a rejected outbound target is a check outcome, not a fake HTTP response.</summary>
public sealed class HttpServiceSafetyTests
{
    [Fact]
    public async Task PrivateAddress_ReturnsTransportErrorWithoutHttpStatus()
    {
        var result = await new HttpService().CheckAsync(
            new Uri("http://127.0.0.1/"), CancellationToken.None);

        Assert.Equal("unsafe_destination", result.ErrorCode);
        Assert.Null(result.StatusCode);
        Assert.Null(result.FinalAddress);
        Assert.False(result.IsSuccess);
    }
}
