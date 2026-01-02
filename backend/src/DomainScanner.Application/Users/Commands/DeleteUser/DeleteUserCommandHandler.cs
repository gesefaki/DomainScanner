using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Users.Exceptions;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(IUsersRepository usersRepository, IUnitOfWork uof) 
    : IRequestHandler<DeleteUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IUnitOfWork _uof = uof;

    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(nameof(User), request.Id);
        
        _usersRepository.Delete(user);
        await _uof.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}