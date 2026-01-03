using DomainScanner.Api.DTOs.Users;
using DomainScanner.Api.Mapping;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Exceptions;
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
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery();
        var users = await _mediator.Send(query, cancellationToken);
        var result = users.Select(UsersMapper.UserToResponseUserDto);
        return Ok(result);
    }

    [HttpGet("{id::guid}")]
    public async Task<ActionResult<UserResponseDto>> Get(Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(id);
        
        var user = await _mediator.Send(query, cancellationToken);

        if (user is null)
            throw new UserNotFoundException(nameof(User), id);
        
        return UsersMapper.UserToResponseUserDto(user);
    }

    [HttpPost("create")]
    public async Task<ActionResult<User>> Create([FromBody]CreateUserDto request,
        CancellationToken cancellationToken = default)
    {
       var user = UsersMapper.CreateUserDtoToUser(request);
       var cmd = new CreateUserCommand(user);
       await _mediator.Send(cmd, cancellationToken);
       return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPatch("activate/{id::guid}")]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            throw new UserNotFoundException(nameof(User), id);

        if (user.IsActive)
            throw new UnableToExecuteException(nameof(user), user.Id, nameof(user.IsActive));

        var cmd = new ActivateUserCommand(user.Id);
        await _mediator.Send(cmd, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }
    
    [HttpPatch("deactivate/{id::guid}")]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            throw new UserNotFoundException(nameof(User), id);

        if (!user.IsActive)
            throw new UnableToExecuteException(nameof(User), user.Id, nameof(user.IsActive));
            
        var cmd = new DeactivateUserCommand(user.Id);
        await _mediator.Send(cmd, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpDelete("delete/{id::guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            throw new UserNotFoundException(nameof(User), id);

        var cmd = new DeleteUserCommand(user.Id);
        await _mediator.Send(cmd, cancellationToken);
        return NoContent();
    }
    
}