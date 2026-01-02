using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Users.Exceptions;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Commands.UnableUser;

public class ActivateUserCommandHandler(IUsersRepository usersRepository, IUnitOfWork uof) 
    : IRequestHandler<ActivateUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IUnitOfWork _uof = uof;

    public async Task<Guid> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(nameof(User), request.Id);

        if (user.IsActive is true)
            throw new UnableToExecuteException(nameof(user), request.Id, user.IsActive);

        user.IsActive = true;
        _usersRepository.Update(user);
        await _uof.SaveChangesAsync(cancellationToken);
        
        return user.Id;
    }
}