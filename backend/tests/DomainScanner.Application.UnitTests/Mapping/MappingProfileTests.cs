using AutoMapper;
using DomainScanner.Application.Mapping;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomainScanner.Application.UnitTests.Mapping;

public sealed class MappingProfileTests
{
    [Fact]
    public void StoredTransportFailure_MapsToProtocolPayloadWithoutHttpStatus()
    {
        var check = new DomainCheckResult
        {
            Id = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            Kind = "http",
            Outcome = "error",
            RequestedAddress = "https://example.com/",
            FinalAddress = null,
            StatusCode = null,
            ErrorCode = "dns_error",
            ResponseTimeMs = 1000,
            CreatedAt = DateTime.UtcNow
        };

        var response = DomainResponseMapping.ToCheck(check);

        Assert.Equal(check.Id, response.Id);
        Assert.Equal("error", response.Outcome);
        Assert.NotNull(response.Http);
        Assert.Null(response.Http.StatusCode);
        Assert.Null(response.Http.FinalAddress);
        Assert.Equal("dns_error", response.Http.ErrorCode);
    }

    [Fact]
    public void DomainResponse_OnlyExposesLatestCheckSummary()
    {
        var domain = new DomainEntity
        {
            Id = Guid.NewGuid(),
            Address = "https://example.com/",
            CheckResults =
            [
                new DomainCheckResult
                {
                    Id = Guid.NewGuid(), Kind = "http", Outcome = "down",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                },
                new DomainCheckResult
                {
                    Id = Guid.NewGuid(), Kind = "http", Outcome = "up",
                    CreatedAt = DateTime.UtcNow
                }
            ]
        };

        var response = DomainResponseMapping.ToDomain(domain);

        Assert.Equal("up", response.LastCheck?.Outcome);
        Assert.Equal("http", response.LastCheck?.Kind);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DomainResponse_MapsMonitoringInsteadOfAvailability(bool monitoringEnabled)
    {
        // Arrange
        var configuration = new MapperConfiguration(
            options => options.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        var domain = new DomainEntity
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Address = "example.com",
            MonitoringEnabled = monitoringEnabled, IsActive = !monitoringEnabled
        };

        // Act
        var response = configuration.CreateMapper().Map<DomainResponse>(domain);

        // Assert
        Assert.Equal(domain.Id, response.Id);
        Assert.Equal(domain.UserId, response.UserId);
        Assert.Equal(domain.Address, response.Address);
        Assert.Equal(monitoringEnabled, response.MonitoringEnabled);
        Assert.Null(response.LastCheck);
    }
}
