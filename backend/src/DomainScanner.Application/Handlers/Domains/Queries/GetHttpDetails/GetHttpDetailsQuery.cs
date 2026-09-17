using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpDetails;

/// <summary>
/// Query to retrieve a detailed HTTP response for a domain owned by the current authenticated user.
/// </summary>
/// <param name="Id">Unique identifier of the domain to check.</param>
public record GetHttpDetailsQuery(Guid Id)
    : IRequest<HttpResponseDetails>, INeedAuthentication;
