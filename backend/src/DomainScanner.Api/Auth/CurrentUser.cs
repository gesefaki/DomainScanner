using System.Security.Claims;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Contracts.Exceptions.Common;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DomainScanner.Api.Auth;

/// <summary>
/// Provides information about the current user based on the HTTP context.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUser"/> class.
    /// </summary>
    /// <param name="accessor">
    /// The HTTP context accessor used to retrieve the current user principal.
    /// </param>
    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }
    
    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;
    
    /// <inheritdoc />
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    /// <exception cref="NonAuthenticatedException">
    /// Thrown when the current principal does not contain a valid
    /// <c>sub</c> claim.
    /// </exception>
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