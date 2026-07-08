using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.DeactivateUser;

/// <summary>
/// Handles <see cref="DeactivateUserCommand"/>. 
/// </summary>
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, UserResponse>
{
    private readonly IRepository<User, Guid> _repository;
    private readonly IMapper _mapper;
    
    public DeactivateUserCommandHandler(IRepository<User, Guid> repository, 
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    /// <inheritdoc />
    public async Task<UserResponse> Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        // Getting user
        var user = await _repository.FindAsync(request.Id, ct);
        if (user is null)
        {
            throw new UserNotFoundException(request.Id);
        }

        // Is user deactivated?
        if (!user.IsActive)
        {
            throw new UnableToExecuteException(nameof(user), user.Id, nameof(user.IsActive));
        }

        // Deactivating
        user.IsActive = false;
        var updatedUser = _repository.Update(user);

        return _mapper.Map<UserResponse>(updatedUser);
    }
}