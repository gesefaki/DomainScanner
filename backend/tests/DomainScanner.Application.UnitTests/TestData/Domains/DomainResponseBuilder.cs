using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.HTTPs.Responses;

namespace DomainScanner.Application.UnitTests.TestData.Domains;

/// <summary>
/// Builder for creating <see cref="DomainResponse"/> instances in tests.
/// By default, returns valid responses with random IDs and default values.
/// </summary>
public sealed class DomainResponseBuilder
{
    private Guid _domainId = Guid.NewGuid();
    private string _address = "https://example.com/";
    private bool? _isActive = true;
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
    /// Sets the domain as active/available.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder Active()
    {
        _isActive = true;
        return this;
    }
    
    /// <summary>
    /// Sets the domain as inactive/unavailable.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainResponseBuilder Inactive()
    {
        _isActive = false;
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
    /// Builds and returns a <see cref="DomainResponse"/> with the configured properties.
    /// </summary>
    /// <returns>A new <see cref="DomainResponse"/> instance.</returns>
    public DomainResponse Build()
    {
        return new DomainResponse(
            Id: _domainId,
            Address: _address,
            IsAvailable: _isActive,
            UserId: _userId,
            Checks: _checks
            );
    }
}