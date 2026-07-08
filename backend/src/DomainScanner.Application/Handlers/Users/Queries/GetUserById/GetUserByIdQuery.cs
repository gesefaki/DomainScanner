using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetUserById;

/// <summary>
/// Query to retrieve single <see cref="UserResponse"/> by its identifier. Cacheable with <see cref="ICacheableQuery"/> interface. 
/// </summary>
/// <param name="Id">User unique identifier.</param>
public record GetUserByIdQuery(Guid Id) : IRequest<UserResponse>, ICacheableQuery;