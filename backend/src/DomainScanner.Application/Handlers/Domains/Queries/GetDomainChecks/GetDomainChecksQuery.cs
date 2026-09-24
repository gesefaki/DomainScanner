using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainChecks;

/// <summary>Retrieves the retained check history of an owned domain.</summary>
/// <param name="DomainId">Identifier of the domain.</param>
public sealed record GetDomainChecksQuery(Guid DomainId)
    : IRequest<IReadOnlyList<DomainCheckResponse>>, INeedAuthentication;
