using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, UserResponse>
{
    private readonly IReadRepository<User> _readRepository;
    private readonly IWriteRepository<User> _writeRepository;
    private readonly IMapper _mapper;
    
    public DeactivateUserCommandHandler(IReadRepository<User> readRepository, 
        IWriteRepository<User> writeRepository, 
        IMapper mapper)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _mapper = mapper;
    }
    
    public async Task<UserResponse> Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        // Getting user
        var user = await _readRepository.FindAsync(request.Request.Id, ct);
        if (user is null)
        {
            throw new UserNotFoundException(request.Request.Id);
        }

        // Is user deactivated?
        if (!user.IsActive)
        {
            throw new UnableToExecuteException(nameof(user), user.Id, nameof(user.IsActive));
        }

        // Deactivating
        user.IsActive = false;
        var updatedUser = _writeRepository.Update(user);

        return _mapper.Map<UserResponse>(updatedUser);
    }
}