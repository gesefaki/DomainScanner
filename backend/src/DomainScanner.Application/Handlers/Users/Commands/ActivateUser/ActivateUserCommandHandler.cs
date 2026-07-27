using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.ActivateUser;

/// <summary>
/// Handles <see cref="ActivateUserCommand"/>.
/// </summary>
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserResponse>
{
    private readonly IRepository<User, Guid> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public ActivateUserCommandHandler(
        IRepository<User, Guid> repository,
        IMapper mapper,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<UserResponse> Handle(ActivateUserCommand request, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        var user = await _repository.FindAsync(userId, ct);

        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        if (user.IsActive)
        {
            throw new UnableToExecuteException(nameof(user), userId, nameof(user.IsActive));
        }

        user.IsActive = true;
        var updatedUser = _repository.Update(user);

        return _mapper.Map<UserResponse>(updatedUser);
    }
}
