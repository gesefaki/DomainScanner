using DomainScanner.Application.Handlers.Users.Commands.ActivateUser;
using DomainScanner.Application.Handlers.Users.Commands.DeactivateUser;
using DomainScanner.Application.Handlers.Users.Commands.DeleteUser;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;
using DomainScanner.Application.Handlers.Users.Queries.GetMyDomainsQuery;
using DomainScanner.Application.Handlers.Users.Queries.GetUserById;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.DTOs.Users.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// Retrieves all users for authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of <see cref="UserResponse"/>. </returns>
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll(CancellationToken ct)
    {
        var users = await _sender.Send(new GetAllUsersQuery(), ct);
        return Ok(users);
    }

    /// <summary>
    /// Retrieves single user for authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="UserResponse"/>.</returns>
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Get(CancellationToken ct)
    {
        var user = await _sender.Send(new GetUserByIdQuery(), ct);
        return Ok(user);
    }

    [HttpGet("me/domains")]
    public async Task<ActionResult<DomainResponse>> GetMyDomains(CancellationToken ct)
    {
        var domains = await _sender.Send(new GetMyDomainsQuery(), ct);
        return Ok(domains);
    }

    /// <summary>
    /// Register a new user account. No authentication needed.
    /// </summary>
    /// <param name="request">Register user request.</param>
    /// <param name="ct">Cancellation token.</param>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var user = await _sender.Send(new RegisterUserCommand(request), ct);
        return CreatedAtAction(nameof(Get), value: user);
    }

    /// <summary>
    /// Activates a user account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="UserResponse"/>.</returns>
    [HttpPut("me/activate")]
    public async Task<ActionResult<UserResponse>> Activate(CancellationToken ct)
    {
        var result = await _sender.Send(new ActivateUserCommand(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="UserResponse"/>.</returns>
    [HttpPut("me/deactivate")]
    public async Task<ActionResult> Deactivate(CancellationToken ct)
    {
        var user = await _sender.Send(new DeactivateUserCommand(), ct);
        return Ok(user);
    }

    /// <summary>
    /// Deletes a user account from database. Not soft delete.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("me")]
    public async Task<ActionResult> Delete(CancellationToken ct)
    {
        await _sender.Send(new DeleteUserCommand(), ct);
        return NoContent();
    }
}
