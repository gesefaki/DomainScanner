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

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : Controller
{
    private readonly IMediator _mediator;
    
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll(CancellationToken ct)
    {
        var users = await _mediator.Send(new GetAllUsersQuery(), ct);
        return Ok(users);
    }

    [HttpGet("{id::guid}")]
    public async Task<ActionResult<UserResponse>> Get([FromBody] GetUserByIdRequest request, CancellationToken ct)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(request), ct);
        return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<User>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var user = await _mediator.Send(new RegisterUserCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("activate/{id::guid}")]
    public async Task<ActionResult> Activate(ActivateUserRequest request, CancellationToken ct)
    {
        var user = await _mediator.Send(new ActivateUserCommand(request), ct);
        return Ok(user);
    }

    [HttpPut("deactivate/{id::guid}")]
    public async Task<ActionResult> Deactivate(DeactivateUserRequest request, CancellationToken ct)
    {
        var user = await _mediator.Send(new DeactivateUserCommand(request), ct);
        return Ok(user);
    }

    [HttpDelete("{id::guid}")]
    public async Task<ActionResult> Delete(DeleteUserRequest request, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserCommand(request), ct);
        return NoContent();
    }

}