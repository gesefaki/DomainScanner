using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetAllDomains;

/// <summary>
/// Query to retrieve all DomainEntity from database. 
/// </summary>
public record GetAllDomainsQuery : IRequest<IEnumerable<DomainResponse>>, ICacheableQuery;