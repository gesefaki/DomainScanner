using DomainScanner.Contracts.Options.RateLimiting;

namespace DomainScanner.Contracts.UnitTests.Options;

public sealed class RateLimitingSettingsTests
{
    [Theory]
    [InlineData(20, 0, true)]
    [InlineData(20, 1, true)]
    [InlineData(0, 0, false)]
    [InlineData(-1, 0, false)]
    [InlineData(20, -1, false)]
    public void IsValid_ValidatesGlobalConcurrency(int permits, int queue, bool expected)
    {
        // Arrange
        var window = new SlidingWindowSettings
        {
            PermitLimit = 10, WindowSeconds = 60, SegmentsPerWindow = 6, QueueLimit = 0
        };
        var settings = new RateLimitingSettings
        {
            Read = window, Write = window, Auth = window, Login = window, Scan = window,
            ScanConcurrency = new ConcurrencySettings { PermitLimit = 3 },
            GlobalScanConcurrency = new ConcurrencySettings { PermitLimit = permits, QueueLimit = queue }
        };

        // Act
        var actual = settings.IsValid();

        // Assert
        Assert.Equal(expected, actual);
    }
}
