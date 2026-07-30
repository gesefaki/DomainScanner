using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.RegisterUser;

/// <summary>
/// Handles <see cref="RegisterUserCommand"/>. Has a <see cref="RegisterUserCommandValidator"/> must be passed. 
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserResponse>
{
    private readonly IRepository<User, Guid> _repository;
    private readonly IPasswordHasher _hasher;
    private readonly IMapper _mapper;
    private readonly IEmailNormalizer _emailNormalizer;

    public RegisterUserCommandHandler(IRepository<User, Guid> repository,
        IPasswordHasher hasher,
        IMapper mapper, IEmailNormalizer emailNormalizer)
    {
        _repository = repository;
        _hasher = hasher;
        _mapper = mapper;
        _emailNormalizer = emailNormalizer;
    }

    /// <inheritdoc />
    public async Task<UserResponse> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        // normalize email to compare
        var normalizedEmail =
            _emailNormalizer.Normalize(request.Request.Email);
        
        // check the existence of a user by credits
        if (await _repository.IsExistsByAttribute(u => u.Email == request.Request.Email, ct) ||
            await _repository.IsExistsByAttribute(u => u.Username == request.Request.Username, ct))
        {
            throw new UserConflictCredsException();
        }

        // hashing password
        var hashedPassword = _hasher.Generate(request.Request.Password);

        // creating new user entity
        var user = new User
        {
            Username = request.Request.Username,
            Email = request.Request.Email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = hashedPassword,
            IsActive = true
        };

        // adding user to db
        await _repository.CreateAsync(user, ct);

        return _mapper.Map<UserResponse>(user);
    }
}
