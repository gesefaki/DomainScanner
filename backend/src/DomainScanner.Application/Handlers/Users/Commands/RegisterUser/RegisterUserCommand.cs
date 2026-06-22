using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Username, string Email, string Password) : IRequest<User>;