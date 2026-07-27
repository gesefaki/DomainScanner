using DomainScanner.Application.Handlers.Users.Queries.LoginUser;
using DomainScanner.Contracts.DTOs.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

/// <summary>
/// REST API controller handles auth operations.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }
    
    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] LoginUserRequest request, CancellationToken ct)
    {
        var context = HttpContext;

        var token = await _sender.Send(new LoginUserQuery(request), ct);
        
        context.Response.Cookies.Append("tasty_cookies", token);

        return Ok(token);
    }
}