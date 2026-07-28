using DomainScanner.Application.Handlers.Users.Queries.LoginUser;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.Options;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.Controllers;

/// <summary>
/// REST API controller handles auth operations.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAntiforgery _antiforgery;

    public AuthController(ISender sender, IAntiforgery antiforgery)
    {
        _sender = sender;
        _antiforgery = antiforgery;
    }
    
    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    [EnableRateLimiting(RateLimitingOptions.Auth)]
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult> Login([FromBody] LoginUserRequest request, CancellationToken ct)
    {
        var context = HttpContext;

        var token = await _sender.Send(new LoginUserQuery(request), ct);
        
        context.Response.Cookies.Append(
            AuthCookieOptions.Session,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromHours(1)
            });

        return NoContent();
    }

    /// <summary>
    /// Logout user and delete his session cookies.
    /// </summary>
    [EnableRateLimiting(RateLimitingOptions.Auth)]
    [HttpPost("logout")]
    public ActionResult Logout()
    {
        var context = HttpContext;

        context.Response.Cookies.Delete(
            AuthCookieOptions.Session,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            }
        );

        return NoContent();
    }
    
    [EnableRateLimiting(RateLimitingOptions.Auth)]
    [AllowAnonymous]
    [HttpGet("csrf")]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public ActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            token = tokens.RequestToken
        });
    }
}