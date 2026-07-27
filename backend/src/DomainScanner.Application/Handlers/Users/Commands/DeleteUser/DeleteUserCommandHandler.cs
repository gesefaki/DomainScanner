using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.DeleteUser;

/// <summary>
/// Handles <see cref="DeleteUserCommand"/>.
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Guid>
{
    private readonly IRepository<User, Guid> _repository;
    private readonly ICurrentUser _currentUser;

    public DeleteUserCommandHandler(
        IRepository<User, Guid> repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        var user = await _repository.FindAsync(userId, ct);

        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        _repository.Delete(user);

        return user.Id;
    }
}
