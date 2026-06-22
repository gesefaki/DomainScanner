using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetAllDomainsByUser;

public record GetAllDomainsByUserQuery(Guid UserId) : IRequest<IEnumerable<DomainResponse>>;