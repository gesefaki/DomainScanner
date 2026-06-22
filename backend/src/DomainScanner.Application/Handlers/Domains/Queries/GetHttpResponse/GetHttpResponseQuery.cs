using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpResponse;

public record GetHttpResponseQuery(Guid Id) : IRequest<HttpResponseObject>;