using System.Security.Claims;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Contracts.Exceptions.Common;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DomainScanner.Api.Auth;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid Id
    {
        get
        {
            var value = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(value, out var id))
            {
                throw new NonAuthenticatedException();
            }

            return id;
        }
    }
}