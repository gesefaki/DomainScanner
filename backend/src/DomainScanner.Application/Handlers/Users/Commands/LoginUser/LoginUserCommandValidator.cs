using DomainScanner.Application.Handlers.Users.Common;
using FluentValidation;

namespace DomainScanner.Application.Handlers.Users.Commands.LoginUser;

/// <summary>
/// Validates the <see cref="LoginUserCommand"/>.
/// Implements FluentValidation's <see cref="AbstractValidator{T}"/>.  
/// </summary>
public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
     /// <summary>
    /// Sets up all validation rules for the <see cref="LoginUserCommand"/>. 
    /// </summary>
    public LoginUserCommandValidator()
    {
        // Email
        RuleFor(r => r.Request.Email)
            .ValidEmail();
        
        // Password
        RuleFor(r => r.Request.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}