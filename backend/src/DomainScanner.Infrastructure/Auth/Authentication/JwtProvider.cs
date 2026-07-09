using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Contracts.Options;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DomainScanner.Infrastructure.Auth.Authentication;

/// <summary>
/// Provides JWT generation functionally for user auth. Implements <see cref="IJwtProvider"/>.
/// </summary>
public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }
    

    /// <inheritdoc />
    public string GenerateToken(User user)
    {
        Claim[] claims = [new("userId", user.Id.ToString())]; 
        
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddHours(_options.ExpiresHours)
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;
    }
}