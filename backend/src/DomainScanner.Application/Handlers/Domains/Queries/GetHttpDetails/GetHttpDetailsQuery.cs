using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpDetails;

public record GetHttpDetailsQuery(Guid Id) : IRequest<HttpResponseDetails>;