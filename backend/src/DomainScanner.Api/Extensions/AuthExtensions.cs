using System.Security.Claims;
using System.Text;
using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.JsonWebTokens;
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
                options.MapInboundClaims = false;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions!.SecretKey)),
                    
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };

                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        var subject = context.Principal?
                            .FindFirstValue(JwtRegisteredClaimNames.Sub);

                        if (!Guid.TryParse(subject, out _))
                        {
                            context.Fail("JWT contains an invalid subject.");
                        }

                        return Task.CompletedTask;
                    },
                    
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(
                                AuthCookieOptions.Session,
                                out var token)
                           )
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                    
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
    }

    public static IServiceCollection AddCsrfProtection(this IServiceCollection services,
        IConfiguration configuration)
    {
        var keysPath = configuration["DataProtection:KeysPath"]
                       ?? "/var/lib/domainscanner/keys";

        services
            .AddDataProtection()
            .SetApplicationName("DomainScanner")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        return services;
    }
}