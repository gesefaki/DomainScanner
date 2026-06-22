namespace DomainScanner.Contracts.DTOs.Users.Requests;

public record RegisterUserRequest(string Username, string Email, string Password);