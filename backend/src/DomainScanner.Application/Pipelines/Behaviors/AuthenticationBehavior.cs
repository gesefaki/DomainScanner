using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.Exceptions.Common;
using MediatR;

namespace DomainScanner.Application.Pipelines.Behaviors;

public sealed class AuthenticationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : INeedAuthentication
{
    private readonly ICurrentUser _currentUser;

    public AuthenticationBehavior(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new NonAuthorizedException();
        }

        var response = await next(ct);

        return response;
    }
}
