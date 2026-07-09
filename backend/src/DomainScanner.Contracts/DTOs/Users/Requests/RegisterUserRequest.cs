namespace DomainScanner.Contracts.DTOs.Users.Requests;

/// <summary>
/// Request to register user.
/// </summary>
/// <param name="Username">Username from user input.</param>
/// <param name="Email">Email from user input.</param>
/// <param name="Password">Password from user input.</param>
public record RegisterUserRequest(string Username, string Email, string Password);