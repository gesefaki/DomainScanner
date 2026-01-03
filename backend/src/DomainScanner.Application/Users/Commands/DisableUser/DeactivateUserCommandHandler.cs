using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Commands.DisableUser;

public class DeactivateUserCommandHandler(IUsersRepository usersRepository, IUnitOfWork uof) 
    : IRequestHandler<DeactivateUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IUnitOfWork _uof = uof;
    
    public async Task<Guid> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(nameof(User), request.Id);

        if (!user.IsActive)
            throw new UnableToExecuteException(nameof(user), user.Id, nameof(user.IsActive));

        user.IsActive = false;
        _usersRepository.Update(user);
        await _uof.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}