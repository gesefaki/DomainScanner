using DomainScanner.Application.Handlers.Users.Common;
using FluentValidation;

namespace DomainScanner.Application.Handlers.Users.Queries.LoginUser;

/// <summary>
/// Validates the <see cref="LoginUserQuery"/>.
/// Implements FluentValidation's <see cref="AbstractValidator{T}"/>.  
/// </summary>
public class LoginUserQueryValidator : AbstractValidator<LoginUserQuery>
{
     /// <summary>
    /// Sets up all validation rules for the <see cref="LoginUserQuery"/>. 
    /// </summary>
    public LoginUserQueryValidator()
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