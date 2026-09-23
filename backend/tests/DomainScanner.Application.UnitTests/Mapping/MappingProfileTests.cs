using AutoMapper;
using DomainScanner.Application.Mapping;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomainScanner.Application.UnitTests.Mapping;

public sealed class MappingProfileTests
{
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
        Assert.Empty(response.Checks);
    }
}
