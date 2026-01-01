using DomainScanner.Api.DTOs.Users;
using DomainScanner.Api.Mapping;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Users.Commands.CreateUser;
using DomainScanner.Application.Users.Commands.DeleteUser;
using DomainScanner.Application.Users.Commands.DisableUser;
using DomainScanner.Application.Users.Commands.UnableUser;
using DomainScanner.Application.Users.Queries.GetAllUsers;
using DomainScanner.Application.Users.Queries.GetUserById;
using DomainScanner.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

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
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery();
        var users = await _mediator.Send(query, cancellationToken);
        var result = users.Select(UsersMapper.UserToResponseUserDto);
        return Ok(result);
    }

    [HttpGet("{id::guid}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(id);
        
        var user = await _mediator.Send(query, cancellationToken);
        
        if(user is null)
            return NotFound();
        
        return UsersMapper.UserToResponseUserDto(user);
    }

    [HttpPost("create")]
    public async Task<ActionResult<User>> CreateUser([FromBody]CreateUserDto request,
        CancellationToken cancellationToken = default)
    {
       var user = UsersMapper.CreateUserDtoToUser(request);
       var cmd = new CreateUserCommand(user);
       await _mediator.Send(cmd, cancellationToken);
       return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPatch("activate/{id::guid}")]
    public async Task<ActionResult> ActivateUser(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            return NotFound();

        if (user.IsActive is true)
            return Conflict();

        var cmd = new ActivateUserCommand(user.Id);
        await _mediator.Send(cmd, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
    
    [HttpPatch("deactivate/{id::guid}")]
    public async Task<ActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            return NotFound();

        if (user.IsActive is false)
            return Conflict();
            
        var cmd = new DeactivateUserCommand(user.Id);
        await _mediator.Send(cmd, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpDelete("delete/{id::guid}")]
    public async Task<ActionResult> DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            return NotFound();

        var cmd = new DeleteUserCommand(user.Id);
        await _mediator.Send(cmd, cancellationToken);
        return NoContent();
    }
    
}