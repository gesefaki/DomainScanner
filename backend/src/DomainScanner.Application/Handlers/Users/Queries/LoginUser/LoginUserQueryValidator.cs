using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;
using FluentValidation;

namespace DomainScanner.Application.Handlers.Users.Queries.LoginUser;

/// <summary>
/// Validates the <see cref="LoginUserQuery"/>.
/// Implements FluentValidation's <see cref="AbstractValidator{T}"/>.  
/// </summary>
public class LoginUserQueryValidator : AbstractValidator<LoginUserQuery>
{
    private readonly IReadRepository<User, Guid> _repository;
    
     /// <summary>
    /// Sets up all validation rules for the <see cref="LoginUserQuery"/>. 
    /// </summary>
    /// <param name="repository">The <see cref="IReadRepository{User, Guid}"/> repository instance needs for check email is unique and password is valid and correct. </param>
    public LoginUserQueryValidator(IReadRepository<User, Guid> repository)
    {
        _repository = repository;

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

    /// <summary>
    /// Validates that the user email is unique.
    /// </summary>
    /// <param name="email">User email as a string.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    private async Task<bool> IsUniqueEmail(string email, CancellationToken ct)
    {
        return !await _repository.IsExistsByAttribute(u => u.Email == email, ct);
    }
}