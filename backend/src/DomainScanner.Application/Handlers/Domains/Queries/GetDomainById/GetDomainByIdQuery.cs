using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;

public record GetDomainByIdQuery(Guid Id) : IRequest<DomainResponse>, ICacheableQuery;