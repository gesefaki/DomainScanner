using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using FluentValidation;

namespace DomainScanner.Application.Validation;

public class UsersValidator : AbstractValidator<User>
{
    private readonly IUsersRepository _usersRepository;
    
    public UsersValidator(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
        
        RuleFor(user => user.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(1, 50).WithMessage("Username must be between 1 and 50 characters");
        
        RuleFor(user => user.Email)
            .MustAsync(async (email, ct) => !await IsEmailUniqueAsync(email, ct))
            .WithMessage("Email already exists");
    }

    private async Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct)
    {
        return await _usersRepository.IsExistsByEmailAsync(email, ct);
    }
}