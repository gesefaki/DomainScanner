using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler(IUsersRepository usersRepository, IUnitOfWork uof) : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IUnitOfWork _uof = uof;
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await _usersRepository.CreateUserAsync(request.User, cancellationToken);
        await  _uof.SaveChangesAsync(cancellationToken);
        return request.User.Id;
    }
}