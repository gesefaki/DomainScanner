using DomainScanner.Api.DTOs.Users;
using DomainScanner.Api.Mapping;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Exceptions;
using DomainScanner.Application.Users.Commands.ActivateUser;
using DomainScanner.Application.Users.Commands.DeactivateUser;
using DomainScanner.Application.Users.Commands.DeleteUser;
using DomainScanner.Application.Users.Commands.RegisterUser;
using DomainScanner.Application.Users.Queries.GetAllUsers;
using DomainScanner.Application.Users.Queries.GetUserById;
using DomainScanner.Application.Users.Queries.LoginUser;
using DomainScanner.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : Controller
{
    private readonly IMediator _mediator;
    private readonly IValidator<User> _validator;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(IMediator mediator, IValidator<User> validator,
        ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _validator = validator;
        _logger = logger;
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
            throw new UserNotFoundException(id);

        return UsersMapper.UserToResponseUserDto(user);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody]RegisterUserDto request,
        CancellationToken ct = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync
            (UsersMapper.RegisterUserDtoToUser(request), ct);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning($"Validation error: {validationResult.Errors.First().ErrorMessage}");
            throw new BadRequestException(validationResult.Errors.ToString()!);
        }

        var cmd = new RegisterUserCommand(request.Username, request.Email, request.Password);
        var user = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, UsersMapper.UserToResponseUserDto(user));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody]LoginUserDto request, CancellationToken ct = default)
    {
        var context = HttpContext;
        
        var query = new LoginUserQuery(request.Email, request.Password);
        
        var token = await _mediator.Send(query, ct);
        
        context.Response.Cookies.Append("tasty_cookies", token);
            
        return Ok(token);
    }

    [HttpPatch("activate/{id::guid}")]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            throw new UserNotFoundException(id);

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
            throw new UserNotFoundException(id);

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
            throw new UserNotFoundException(id);

        var cmd = new DeleteUserCommand(user.Id);
        await _mediator.Send(cmd, cancellationToken);
        return NoContent();
    }
    
}