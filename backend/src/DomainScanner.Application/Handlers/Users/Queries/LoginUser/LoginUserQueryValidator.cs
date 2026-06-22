using DomainScanner.Application.Abstractions.Persistence;
using FluentValidation;

namespace DomainScanner.Application.Handlers.Users.Queries.LoginUser;

public class LoginUserQueryValidator : AbstractValidator<LoginUserQuery>
{
    private readonly IUsersRepository _repository;
    
    public LoginUserQueryValidator(IUsersRepository repository)
    {
        _repository = repository;

        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(IsUniqueEmail).WithMessage("Email already registered.");

        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");
    }

    private async Task<bool> IsUniqueEmail(string email, CancellationToken ct)
    {
        return !await _repository.IsExistsByEmailAsync(email, ct);
    }
}