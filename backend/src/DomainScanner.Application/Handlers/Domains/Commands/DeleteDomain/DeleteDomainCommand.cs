using DomainScanner.Application.Pipelines.Interfaces;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;

/// <summary>
/// Command to delete a domain owned by the current authenticated user from the database.
/// </summary>
/// <param name="Id">Unique identifier of the domain to delete.</param>
public record DeleteDomainCommand(Guid Id)
    : ICommand<Unit>, INeedAuthentication;
