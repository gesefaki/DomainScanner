using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetMyDomainsQuery;

/// <summary>
/// Retrieves domain summaries for the current user without embedding check history.
/// </summary>
public record GetMyDomainsQuery() : IRequest<IEnumerable<DomainResponse>>, INeedAuthentication;
