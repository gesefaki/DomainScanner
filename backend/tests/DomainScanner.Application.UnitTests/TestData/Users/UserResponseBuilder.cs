using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.UnitTests.TestData.Users;

/// <summary>
/// Builds <see cref="UserResponse"/> instances for unit tests.
/// </summary>
public sealed class UserResponseBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _username = "test_user";
    private string _email = "user@example.com";
    private bool _isActive = true;

    /// <summary>Sets the user identifier.</summary>
    public UserResponseBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the username.</summary>
    public UserResponseBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    /// <summary>Sets the email.</summary>
    public UserResponseBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>Marks the response as active.</summary>
    public UserResponseBuilder Active()
    {
        _isActive = true;
        return this;
    }

    /// <summary>Marks the response as inactive.</summary>
    public UserResponseBuilder Inactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>Builds a response from a user entity.</summary>
    public static UserResponse Build(User user) => new(user.Id, user.Username, user.Email, user.IsActive, []);

    /// <summary>Builds a new configured response.</summary>
    public UserResponse Build() => new(_id, _username, _email, _isActive, []);
}