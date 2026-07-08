using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpResponse;

/// <summary>
/// Query to retrieve detailed HTTP response from linked DomainEntity
/// </summary>
/// <param name="Id">DomainEntity unique identifier.</param>
public record GetHttpResponseQuery(Guid Id) : IRequest<HttpResponseObject>;