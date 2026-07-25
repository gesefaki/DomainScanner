using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.UnitTests.TestData.Users;

/// <summary>
/// Builds <see cref="User"/> instances for unit tests.
/// </summary>
public sealed class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _username = "test_user";
    private string _email = "user@example.com";
    private string _passwordHash = "hashed-password";
    private bool _isActive = true;

    /// <summary>
    /// Sets the user identifier.
    /// </summary>
    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the username.
    /// </summary>
    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    /// <summary>
    /// Sets the email.
    /// </summary>
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>Sets the password hash.</summary>
    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    /// <summary>Marks the user as active.</summary>
    public UserBuilder Active()
    {
        _isActive = true;
        return this;
    }

    /// <summary>Marks the user as inactive.</summary>
    public UserBuilder Inactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>Builds a new configured user.</summary>
    public User Build() => new()
    {
        Id = _id,
        Username = _username,
        Email = _email,
        PasswordHash = _passwordHash,
        IsActive = _isActive
    };
}