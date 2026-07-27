using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetMyDomainsQuery;

/// <summary>
/// Query to retrieve all DomainEntities by associated user id
/// </summary>
public record GetMyDomainsQuery() : IRequest<IEnumerable<DomainResponse>>, INeedAuthentication;
