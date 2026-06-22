using DomainScanner.Application.Handlers.Users.Queries.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] LoginUserQuery request, CancellationToken ct)
    {
        var context = HttpContext;

        var token = await _mediator.Send(request, ct);
        
        context.Response.Cookies.Append("tasty_cookies", token);

        return Ok(token);
    }
}