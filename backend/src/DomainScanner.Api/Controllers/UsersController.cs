using DomainScanner.Application.Handlers.Users.Commands.DeleteUser;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;
using DomainScanner.Application.Handlers.Users.Queries.GetMyDomainsQuery;
using DomainScanner.Application.Handlers.Users.Queries.GetUserById;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.Controllers;

/// <summary>
/// REST API controller handles user management operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : Controller
{
    private readonly ISender _sender;
    
    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves single user for authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="UserResponse"/>.</returns>
    [EnableRateLimiting(RateLimitingSettings.Policies.Read)]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Get(CancellationToken ct)
    {
        var user = await _sender.Send(new GetUserByIdQuery(), ct);
        return Ok(user);
    }

    /// <summary>Gets domain summaries owned by the current user.</summary>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>Domain settings and their latest check summaries.</returns>
    [EnableRateLimiting(RateLimitingSettings.Policies.Read)]
    [HttpGet("me/domains")]
    public async Task<ActionResult<List<DomainResponse>>> GetMyDomains(CancellationToken ct)
    {
        var domains = await _sender.Send(new GetMyDomainsQuery(), ct);
        return Ok(domains);
    }

    /// <summary>
    /// Register a new user account. No authentication needed.
    /// </summary>
    /// <param name="request">Register user request.</param>
    /// <param name="ct">Cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Auth)]
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var user = await _sender.Send(new RegisterUserCommand(request), ct);
        return CreatedAtAction(nameof(Get), value: user);
    }

    /// <summary>
    /// Deletes a user account from database. Not soft delete.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Write)]
    [HttpDelete("me")]
    public async Task<ActionResult> Delete(CancellationToken ct)
    {
        await _sender.Send(new DeleteUserCommand(), ct);
        return NoContent();
    }
}
