using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Application.Handlers.Users.Queries.LoginUser;
using DomainScanner.Contracts.DTOs.Users.Requests;

namespace DomainScanner.Application.UnitTests.TestData.Users;

/// <summary>
/// Builds user registration and login commands for unit tests.
/// </summary>
public sealed class UserCommandBuilder
{
    private string _username = "test_user";
    private string _email = "user@example.com";
    private string _password = "Password1";

    /// <summary>Sets the username.</summary>
    public UserCommandBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    /// <summary>Sets the email.</summary>
    public UserCommandBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>Sets the plain text password.</summary>
    public UserCommandBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>Builds a registration command.</summary>
    public RegisterUserCommand BuildRegisterCommand() => new(new RegisterUserRequest(_username, _email, _password));

    /// <summary>Builds a login command.</summary>
    public LoginUserQuery BuildLoginQuery() => new(new LoginUserRequest(_email, _password));
}