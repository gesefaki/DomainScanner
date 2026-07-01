using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;
using FluentValidation;

namespace DomainScanner.Application.Handlers.Users.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IReadRepository<User, Guid> _repository;
    
    public RegisterUserCommandValidator(IReadRepository<User, Guid> repository)
    {
        _repository = repository;

        RuleFor(r => r.Request.Username)
            .NotEmpty()
            .Length(3, 20)
            .Matches("^[a-zA-Z0-9_-]+$");

        RuleFor(r => r.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(IsUniqueEmail).WithMessage("Email already registered.");

        RuleFor(r => r.Request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");
    }

    private async Task<bool> IsUniqueEmail(string email, CancellationToken ct)
    {
        return !await _repository.IsExistsByAttribute(u => u.Email == email, ct);
    }
}