using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpResponse;

/// <summary>
/// Query to retrieve a basic HTTP response for a domain owned by the current authenticated user.
/// </summary>
/// <param name="Id">Unique identifier of the domain to check.</param>
public record GetHttpResponseQuery(Guid Id)
    : IRequest<HttpResponseObject>, INeedAuthentication;
