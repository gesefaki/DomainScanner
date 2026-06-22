using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Guid>
{
    private readonly IReadRepository<User> _readRepository;
    private readonly IWriteRepository<User> _writeRepository;

    public DeleteUserCommandHandler(IWriteRepository<User> writeRepository, 
        IReadRepository<User> readRepository)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
    }
    
    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        // Getting user
        var user = await _readRepository.FindAsync(request.Request.Id, ct);
        if (user is null)
        {
            throw new UserNotFoundException(request.Request.Id);
        }
        
        // Deleting
        _writeRepository.Delete(user);
        
        return user.Id;
    }
}