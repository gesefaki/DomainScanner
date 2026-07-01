using DomainScanner.Application.Handlers.Users.Queries.LoginUser;
using DomainScanner.Contracts.DTOs.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] LoginUserRequest request, CancellationToken ct)
    {
        var context = HttpContext;

        var token = await _sender.Send(new LoginUserQuery(request), ct);
        
        context.Response.Cookies.Append("tasty_cookies", token);

        return Ok(token);
    }
}