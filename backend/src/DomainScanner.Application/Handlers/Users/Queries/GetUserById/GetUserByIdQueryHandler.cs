using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetUserById;

/// <summary>
/// Handles <see cref="GetUserByIdQuery"/>.
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IReadRepository<User, Guid> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public GetUserByIdQueryHandler(
        IReadRepository<User, Guid> repository,
        IMapper mapper,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<UserResponse> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        var result = await _repository.FindAsync(userId, ct);

        if (result is null)
        {
            throw new UserNotFoundException(userId);
        }

        return _mapper.Map<UserResponse>(result);
    }
}
