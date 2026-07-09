using System.Text;
using DomainScanner.Contracts.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DomainScanner.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring authentication in the presentation layer.
/// </summary>
public static class AuthExtension
{
    /// <summary>
    /// Configures JWT Bearer auth.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add auth services to.</param>
    /// <param name="configuration">The application configuration containing JWT settings.</param>
    public static void AddApiAuthentication(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection("JwtOptions")
            .Get<JwtOptions>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions!.SecretKey))
                };

                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["tasty_cookies"];
                        return Task.CompletedTask;
                    }
                    
                };
            });

        services.AddAuthorization();
    }
}