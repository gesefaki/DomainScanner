using DomainScanner.Contracts.DTOs.Users.Requests;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.LoginUser;

/// <summary>
/// Command for auth a user and generating an access token.
/// </summary>
/// <param name="Request">A <see cref="LoginUserRequest"/> DTO that stores the data required for login.</param>

public record LoginUserCommand(LoginUserRequest Request) : IRequest<string>;