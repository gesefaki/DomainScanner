using DomainScanner.Application.Pipelines.Interfaces;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;

/// <summary>
/// Command to delete DomainEntity from database.
/// </summary>
/// <param name="Id">Unique identifier of DomainEntity which needs to be deleted.</param>
public record DeleteDomainCommand(Guid Id) : ICommand<Unit>;