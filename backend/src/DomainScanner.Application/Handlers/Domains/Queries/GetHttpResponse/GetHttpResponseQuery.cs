using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;

namespace DomainScanner.Application.Domains.Queries.GetHttpResponse;

public record GetHttpResponseQuery(DomainEntity Domain) : IRequest<HttpResponseObject>;