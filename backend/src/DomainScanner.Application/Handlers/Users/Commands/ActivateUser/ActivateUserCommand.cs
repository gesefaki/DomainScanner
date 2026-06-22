using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.DTOs.Users.Responses;

namespace DomainScanner.Application.Handlers.Users.Commands.ActivateUser;

public record ActivateUserCommand(ActivateUserRequest Request) : ICommand<UserResponse>;