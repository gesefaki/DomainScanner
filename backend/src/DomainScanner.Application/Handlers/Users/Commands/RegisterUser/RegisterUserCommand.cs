using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.DTOs.Users.Responses;

namespace DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
/// <summary>
/// Command to register user and add this user to database.
/// </summary>
/// <param name="Request">A <see cref="RegisterUserRequest"/> DTO that stores the data required for registration.</param>
public record RegisterUserCommand(RegisterUserRequest Request) : ICommand<UserResponse>;