using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetUserById;

/// <summary>
/// Query to retrieve the authenticated user.
/// </summary>
public record GetUserByIdQuery : IRequest<UserResponse>, INeedAuthentication;
