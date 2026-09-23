using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;

/// <summary>
/// Command to update a domain owned by the current authenticated user in the database.
/// </summary>
/// <param name="Id">Unique identifier of the domain to update.</param>
/// <param name="Request"><see cref="UpdateDomainRequest"/> containing the updated address and monitoring setting.</param>
public record UpdateDomainCommand(Guid Id, UpdateDomainRequest Request)
    : ICommand<DomainResponse>, INeedAuthentication;
