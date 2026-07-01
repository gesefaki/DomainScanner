using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Guid>
{
    private readonly IRepository<User, Guid> _repository;

    public DeleteUserCommandHandler(IRepository<User, Guid> repository)
    {
        _repository = repository;
    }
    
    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        // Getting user
        var user = await _repository.FindAsync(request.Id, ct);
        if (user is null)
        {
            throw new UserNotFoundException(request.Id);
        }
        
        // Deleting
        _repository.Delete(user);
        
        return user.Id;
    }
}