using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSaveOwned;

/// <summary>Runs and persists an HTTP check of a domain owned by the current user.</summary>
/// <param name="Id">Identifier of the domain to check.</param>
public sealed record HttpSendAndSaveOwnedCommand(Guid Id)
    : ICommand<DomainCheckResponse>, INeedAuthentication;
