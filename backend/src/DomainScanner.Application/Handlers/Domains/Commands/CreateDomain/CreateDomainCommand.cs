using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;

/// <summary>
/// Command to create new DomainEntity in database
/// </summary>
/// <param name="Request"><see cref="CreateDomainRequest"/> DTO with Address(<c>string</c>) and UserId (<c>struct</c>).</param>
public record CreateDomainCommand(CreateDomainRequest Request) : ICommand<DomainResponse>;