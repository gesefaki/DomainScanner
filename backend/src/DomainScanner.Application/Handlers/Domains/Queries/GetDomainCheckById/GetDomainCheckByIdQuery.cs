using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainCheckById;

/// <summary>Retrieves one stored check of an owned domain.</summary>
/// <param name="DomainId">Identifier of the domain.</param>
/// <param name="CheckId">Identifier of the stored check.</param>
public sealed record GetDomainCheckByIdQuery(Guid DomainId, Guid CheckId)
    : IRequest<DomainCheckResponse>, INeedAuthentication;
