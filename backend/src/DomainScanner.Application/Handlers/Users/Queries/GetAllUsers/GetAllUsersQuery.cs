using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;

public record GetAllUsersQuery() : IRequest<IEnumerable<UserResponse>>, ICacheableQuery;