using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserResponse>
{
    private readonly IRepository<User, Guid> _repository;
    private readonly IMapper _mapper;
    
    public ActivateUserCommandHandler(IRepository<User, Guid> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserResponse> Handle(ActivateUserCommand request, CancellationToken ct)
    {
        // Getting user
        var user = await _repository.FindAsync(request.Id, ct);
        if (user is null)
        {
            throw new UserNotFoundException(request.Id);
        }
        
        // Is user activated?
        if (user.IsActive)
        {
            throw new UnableToExecuteException(nameof(user), request.Id, nameof(user.IsActive));
        }

        // Activation
        user.IsActive = true;
        var updatedUser = _repository.Update(user);

        return _mapper.Map<UserResponse>(updatedUser);
    }
}