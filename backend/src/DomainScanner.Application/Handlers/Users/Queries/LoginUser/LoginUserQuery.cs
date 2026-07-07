using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Requests;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.LoginUser;

public record LoginUserQuery(LoginUserRequest Request) : IRequest<string>, ICacheableQuery;