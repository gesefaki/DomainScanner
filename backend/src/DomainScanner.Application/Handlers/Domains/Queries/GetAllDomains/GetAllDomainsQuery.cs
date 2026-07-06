using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetAllDomains;

public record GetAllDomainsQuery : IRequest<IEnumerable<DomainResponse>>, ICacheable;