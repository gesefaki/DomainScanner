namespace DomainScanner.Contracts.DTOs.Users;

public record RegisterUserRequest(string Username, string Email, string Password);