using DomainScanner.Application.Handlers.Users.Commands.ActivateUser;
using DomainScanner.Application.Handlers.Users.Commands.DeactivateUser;
using DomainScanner.Application.Handlers.Users.Commands.DeleteUser;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;
using DomainScanner.Application.Handlers.Users.Queries.GetUserById;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Domain.Entities;
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
    /// <param name="id">User unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="UserResponse"/>.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Get(Guid id, CancellationToken ct)
    {
        var user = await _sender.Send(new GetUserByIdQuery(id), ct);
        return Ok(user);
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
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    /// <summary>
    /// Activates a user account.
    /// </summary>
    /// <param name="id">User unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="UserResponse"/>.</returns>
    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult<UserResponse>> Activate(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ActivateUserCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="id">User unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="UserResponse"/>.</returns>
    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var user = await _sender.Send(new DeactivateUserCommand(id), ct);
        return Ok(user);
    }

    /// <summary>
    /// Deletes a user account from database. Not soft delete.
    /// </summary>
    /// <param name="id">User unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
}