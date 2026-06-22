using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Models;

namespace DomainScanner.Application.Domains.Queries.GetHttpDetails;

public record GetHttpDetailsQuery(Guid Id) : IRequest<HttpResponseDetails>;