using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;
using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Contracts.DTOs.Domains.Requests;

namespace DomainScanner.Application.UnitTests.TestData.Domains;

/// <summary>
/// Builder for creating domain commands in tests.
/// By default, returns valid commands with random IDs and default values.
/// </summary>
public sealed class DomainCommandBuilder
{
    private Guid _domainId = Guid.NewGuid();
    private string _address = "https://example.com/";
    private bool _monitoringEnabled = true;

    /// <summary>
    /// Sets the domain ID.
    /// </summary>
    /// <param name="domainId">The domain identifier.</param>
    /// <returns>The current builder instance.</returns>
    public DomainCommandBuilder WithId(Guid domainId)
    {
        _domainId = domainId;
        return this;
    }
    
    /// <summary>
    /// Sets the domain address.
    /// </summary>
    /// <param name="address">The domain URL.</param>
    /// <returns>The current builder instance.</returns>
    public DomainCommandBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }
    
    /// <summary>
    /// Enables automatic monitoring without changing the measured availability.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainCommandBuilder EnableMonitoring()
    {
        _monitoringEnabled = true;
        return this;
    }

    /// <summary>
    /// Disables automatic monitoring without changing the measured availability.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DomainCommandBuilder DisableMonitoring()
    {
        _monitoringEnabled = false;
        return this;
    }
    
    /// <summary>
    /// Builds a <see cref="CreateDomainCommand"/> with the configured properties.
    /// </summary>
    /// <returns>A new <see cref="CreateDomainCommand"/> instance.</returns>
    public CreateDomainCommand BuildCreateCommand()
    {
        return new CreateDomainCommand(
            new CreateDomainRequest(
                Address: _address)
        );
    }
    
    /// <summary>
    /// Builds an <see cref="UpdateDomainCommand"/> with the configured properties.
    /// </summary>
    /// <returns>A new <see cref="UpdateDomainCommand"/> instance.</returns>
    public UpdateDomainCommand BuildUpdateCommand()
    {
        return new UpdateDomainCommand(
            _domainId,
            new UpdateDomainRequest(
                Address: _address,
                MonitoringEnabled: _monitoringEnabled)
        );
    }
    
    /// <summary>
    /// Builds a <see cref="DeleteDomainCommand"/> with the configured ID.
    /// </summary>
    /// <returns>A new <see cref="DeleteDomainCommand"/> instance.</returns>
    public DeleteDomainCommand BuildDeleteCommand()
    {
        return new DeleteDomainCommand(_domainId);
    }
}
