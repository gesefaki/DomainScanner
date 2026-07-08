using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;

/// <summary>
/// Query to retrieve all users from database. Cacheable with <see cref="ICacheableQuery"/> interface. 
/// </summary>
public record GetAllUsersQuery() : IRequest<IEnumerable<UserResponse>>, ICacheableQuery;