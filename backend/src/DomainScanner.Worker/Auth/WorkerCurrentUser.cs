using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Contracts.Exceptions.Common;

namespace DomainScanner.Worker.Auth;

/// <summary>
/// Represents the absence of an HTTP user in the background worker process.
/// </summary>
public sealed class WorkerCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public Guid Id => throw new NonAuthenticatedException();
}
