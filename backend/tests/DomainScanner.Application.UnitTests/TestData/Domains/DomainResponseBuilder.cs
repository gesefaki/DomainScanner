using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.HTTPs.Responses;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.UnitTests.TestData.Domains;

/// <summary>
/// Builder for creating <see cref="DomainResponse"/> instances in tests.
/// By default, returns valid responses with random IDs and default values.
/// </summary>
public sealed class DomainResponseBuilder
{
    private Guid _domainId = Guid.NewGuid();
    private string _address = "https://example.com/";
    private bool? _monitoringEnabled = true;
    private Guid _userId = Guid.NewGuid();
    private IEnumerable<HttpResponse> _checks = [];

    /// <summary>
    /// Sets the domain ID.
    /// </summary>
    /// <param name="domainId">The domain identifier.</param>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder WithId(Guid domainId)
    {
        _domainId = domainId;
        return this;
    }

    /// <summary>
    /// Sets the domain address.
    /// </summary>
    /// <param name="address">The domain URL.</param>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    /// <summary>
    /// Enables automatic monitoring in the response.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder EnableMonitoring()
    {
        _monitoringEnabled = true;
        return this;
    }
    
    /// <summary>
    /// Disables automatic monitoring in the response.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder DisableMonitoring()
    {
        _monitoringEnabled = false;
        return this;
    }
    
    /// <summary>
    /// Sets the user ID associated with the domain.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }
    
    /// <summary>
    /// Sets the HTTP check responses for the domain.
    /// </summary>
    /// <param name="checks">Collection of HTTP responses.</param>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder WithChecks(IEnumerable<HttpResponse> checks)
    {
        _checks = checks;
        return this;
    }
    
    /// <summary>
    /// Build and returns a <see cref="DomainResponse"/> based on the provided <see cref="DomainEntity"/>.
    /// </summary>
    /// <param name="baseEntity">The source domain, including its monitoring preference.</param>
    /// <returns>A new <see cref="DomainResponse"/> instance.</returns>
    public DomainResponse Build(DomainEntity baseEntity)
    {
        return new DomainResponse(
            Id: baseEntity.Id,
            Address: baseEntity.Address,
            MonitoringEnabled: baseEntity.MonitoringEnabled,
            UserId: baseEntity.UserId,
            Checks: []
            );
    }
    
    /// <summary>
    /// Builds and returns a <see cref="DomainResponse"/> with the configured properties.
    /// </summary>
    /// <returns>A new <see cref="DomainResponse"/> instance.</returns>
    public DomainResponse Build()
    {
        return new DomainResponse(
            Id: _domainId,
            Address: _address,
            MonitoringEnabled: _monitoringEnabled,
            UserId: _userId,
            Checks: _checks
            );
    }
}
