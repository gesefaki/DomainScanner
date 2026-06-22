using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserResponse>
{
    private readonly IReadRepository<User> _readRepository;
    private readonly IWriteRepository<User> _writeRepository;
    private readonly IPasswordHasher _hasher;
    private readonly IMapper _mapper;

    public RegisterUserCommandHandler(IReadRepository<User> readRepository,
        IWriteRepository<User> writeRepository,
        IPasswordHasher hasher,
        IMapper mapper)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _hasher = hasher;
        _mapper = mapper;
    }

    public async Task<UserResponse> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        // check the existence of a user by credits
        if (await _readRepository.IsExistsByAttribute(u => u.Email == request.Request.Email, ct) &&
            await _readRepository.IsExistsByAttribute(u => u.Username == request.Request.Username, ct))
        {
            throw new UserConflictCredsException();
        }

        // hashing password
        var hashedPassword = _hasher.Generate(request.Request.Password);

        // creating new user entity
        var user = new User
        {
            Username = request.Request.Username,
            PasswordHash = hashedPassword,
            Email = request.Request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true
        };

        // adding user to db
        await _writeRepository.CreateAsync(user, ct);

        return _mapper.Map<UserResponse>(user);
    }
}