namespace DomainScanner.Contracts.DTOs.Domains.Requests;

/// <summary>
/// Request to update an existing domain's address and scheduled monitoring setting.
/// </summary>
/// <param name="Address">Domain URL.</param>
/// <param name="MonitoringEnabled">Whether scheduled monitoring is enabled. Does not change measured availability.</param>
public record UpdateDomainRequest(string Address, bool MonitoringEnabled);
