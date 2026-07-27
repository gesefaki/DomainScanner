namespace DomainScanner.Application.Abstractions.Auth;

public interface ICurrentUser
{
    bool IsAuthenticated { get;  }
    Guid Id { get; }
}