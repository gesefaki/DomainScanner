using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.UnitTests.TestData.Domains;

/// <summary>Builds protocol-neutral domain responses for tests.</summary>
public sealed class DomainResponseBuilder
{
    private Guid _domainId = Guid.NewGuid();
    private string _address = "https://example.com/";
    private bool _monitoringEnabled = true;
    private Guid _userId = Guid.NewGuid();
    private DomainCheckSummaryResponse? _lastCheck;

    /// <summary>Sets the domain identifier.</summary>
    public DomainResponseBuilder WithId(Guid id) { _domainId = id; return this; }

    /// <summary>Sets the configured address.</summary>
    public DomainResponseBuilder WithAddress(string address) { _address = address; return this; }

    /// <summary>Enables scheduled monitoring.</summary>
    public DomainResponseBuilder EnableMonitoring() { _monitoringEnabled = true; return this; }

    /// <summary>Disables scheduled monitoring.</summary>
    public DomainResponseBuilder DisableMonitoring() { _monitoringEnabled = false; return this; }

    /// <summary>Sets the owner identifier.</summary>
    public DomainResponseBuilder WithUserId(Guid id) { _userId = id; return this; }

    /// <summary>Sets the latest check summary.</summary>
    public DomainResponseBuilder WithLastCheck(DomainCheckSummaryResponse? check)
    { _lastCheck = check; return this; }

    /// <summary>Builds a response from a domain entity without loading history.</summary>
    public DomainResponse Build(DomainEntity domain) =>
        new(domain.Id, domain.Address, domain.MonitoringEnabled, domain.UserId, null);

    /// <summary>Builds the configured response.</summary>
    public DomainResponse Build() =>
        new(_domainId, _address, _monitoringEnabled, _userId, _lastCheck);
}
