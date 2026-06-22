using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.DTOs.Users.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetUserById;

public record GetUserByIdQuery(GetUserByIdRequest Request) : IRequest<UserResponse>;