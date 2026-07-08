using DomainScanner.Domain.Models;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetHttpDetails;

/// <summary>
/// Query to retrieve base HTTP response from linked DomainEntity
/// </summary>
/// <param name="Id">DomainEntity unique identifier.</param>
public record GetHttpDetailsQuery(Guid Id) : IRequest<HttpResponseDetails>;