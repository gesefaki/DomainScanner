namespace DomainScanner.Contracts.DTOs.Users.Requests;

/// <summary>
/// Request to login user.
/// </summary>
/// <param name="Email">Email from user input.</param>
/// <param name="Password">Password from user input.</param>
public record LoginUserRequest(string Email, string Password);