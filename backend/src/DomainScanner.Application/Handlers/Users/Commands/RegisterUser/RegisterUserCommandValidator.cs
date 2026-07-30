using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Users.Common;
using DomainScanner.Domain.Entities;
using FluentValidation;

namespace DomainScanner.Application.Handlers.Users.Commands.RegisterUser;

/// <summary>
/// Validates the <see cref="RegisterUserCommand"/>.
/// Implements FluentValidation's <see cref="AbstractValidator{T}"/>.  
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IReadRepository<User, Guid> _repository;
    private readonly IEmailNormalizer _emailNormalizer;
    
    /// <summary>
    /// Sets up all validation rules for the <see cref="RegisterUserCommand"/>. 
    /// </summary>
    /// <param name="repository">The <see cref="IReadRepository{User, Guid}"/> repository instance needs for check user email is unique. </param>
    /// <param name="emailNormalizer">Email normalizer.</param>
    public RegisterUserCommandValidator(IReadRepository<User, Guid> repository,
        IEmailNormalizer emailNormalizer)
    {
        _repository = repository;
        _emailNormalizer = emailNormalizer;
        
        // Email
        RuleFor(x => x.Request.Email)
            .Cascade(CascadeMode.Stop)
            .ValidEmail()
            .MaximumLength(254)
            .MustAsync(IsUniqueEmail)
            .WithMessage("Email already registered.");
        
        // Username
        RuleFor(x => x.Request.Username)
            // Empty
            .NotEmpty()
            .WithMessage("Username cannot be empty.")

            // Length
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters.");
        
        // Password
        RuleFor(x => x.Request.Password)
            .StrongPassword();
        

    }

    /// <summary>
    /// Validates that the user email is unique.
    /// </summary>
    /// <param name="email">User email as a string.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns><c>true</c> if email is unique, otherwise <c>false</c>.</returns>
    private async Task<bool> IsUniqueEmail(string email, CancellationToken ct)
    {
        var normalizedEmail = _emailNormalizer.Normalize(email);
        
        return !await _repository.IsExistsByAttribute(u => u.NormalizedEmail == normalizedEmail, ct);
    }
}