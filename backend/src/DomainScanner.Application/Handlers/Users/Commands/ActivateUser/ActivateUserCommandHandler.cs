using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserResponse>
{
    private readonly IReadRepository<User> _readRepository;
    private readonly IWriteRepository<User> _writeRepository;
    private readonly IMapper _mapper;
    
    public ActivateUserCommandHandler(IWriteRepository<User> writeRepository, 
        IReadRepository<User> readRepository, 
        IMapper mapper)
    {
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _mapper = mapper;
    }

    public async Task<UserResponse> Handle(ActivateUserCommand request, CancellationToken ct)
    {
        // Getting user
        var user = await _readRepository.FindAsync(request.Request.Id, ct);
        if (user is null)
        {
            throw new UserNotFoundException(request.Request.Id);
        }
        
        // Is user activated?
        if (user.IsActive)
        {
            throw new UnableToExecuteException(nameof(user), request.Request.Id, nameof(user.IsActive));
        }

        // Activation
        user.IsActive = true;
        var updatedUser = _writeRepository.Update(user);

        return _mapper.Map<UserResponse>(updatedUser);
    }
}