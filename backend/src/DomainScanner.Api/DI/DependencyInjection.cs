using DomainScanner.Api.Extensions;
using DomainScanner.Api.Middleware;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;

namespace DomainScanner.Api.DI;

/// <summary>
/// Provides extension methods for configuring the presentation layer of the application.
/// Centralizes all API-related dependency injection and middleware configuration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///  Registers all presentation-layer services with the DI container.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddPresentationLayer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173",
                        "https://127.0.0.1:5173",
                        "http://frontend:3000"
                    )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); ;
            });
        });

        services.AddControllers(options =>
        {
            options.Conventions.Add(
                new RouteTokenTransformerConvention(
                    new KebabCaseParameterTransformer()));
        })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            c.UseAllOfToExtendReferenceSchemas();
        });

        services.AddApiAuthentication(configuration);

        services.AddHealthCheck(configuration);

        return services;
    }

    /// <summary>
    /// Utilizes registered in DI container services in application.
    /// </summary>
    public static WebApplication UsePresentationLayer(this WebApplication app)
    {
        app.UseExceptionHandlerMiddleware();

        app.UseCors("Frontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
            HttpOnly = HttpOnlyPolicy.Always,
            Secure = CookieSecurePolicy.Always
        });

        app.UseHealthCheck();

        return app;
    }
}