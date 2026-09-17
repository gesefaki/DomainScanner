using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSaveOwned;

/// <summary>
/// Command to execute and persist an HTTP check for a domain owned by the current authenticated user.
/// </summary>
/// <param name="Id">Unique identifier of the domain to check.</param>
public record HttpSendAndSaveOwnedCommand(Guid Id)
    : ICommand<DomainCheckResult>, INeedAuthentication;
