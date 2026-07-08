using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;

/// <summary>
/// Command to update DomainEntity in database.
/// </summary>
/// <param name="Id">Unique identifier of DomainEntity which need to be update.</param>
/// <param name="Request"><see cref="UpdateDomainRequest"/> DTO with Address(<c>string</c>) and IsActive(<c>bool</c>)</param>
public record UpdateDomainCommand(Guid Id, UpdateDomainRequest Request) : ICommand<DomainResponse>;