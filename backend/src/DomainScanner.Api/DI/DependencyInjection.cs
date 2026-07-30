using DomainScanner.Api.Auth;
using DomainScanner.Api.Configuration;
using DomainScanner.Api.Extensions;
using DomainScanner.Api.Middleware;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Contracts.Options;
using DomainScanner.Infrastructure.Auth.Authentication.Normalization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi;

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
                    .AllowCredentials();
                ;
            });
        });

        services.AddProxy(configuration);

        services.AddAndConfigureRateLimiter();
        
        services.AddCsrfProtection(configuration);

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";

            options.Cookie.Name = AuthCookieOptions.Antiforgery;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
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

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddSingleton<IEmailNormalizer, EmailNormalizer>();

        services.AddApiAuthentication(configuration);

        services.AddHealthCheck(configuration);

        return services;
    }

    /// <summary>
    /// Utilizes registered in DI container services in application.
    /// </summary>
    public static WebApplication UsePresentationLayer(this WebApplication app)
    {
        app.UseForwardedHeaders();
        
        app.UseExceptionHandlerMiddleware();

        app.UseRouting();
        
        app.UseCors("Frontend");

        if (app.Environment.IsDevelopment())
        {
            app.UseStaticFiles();
            
            app.UseSwagger();
            
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "DomainScanner API v1");

                options.ConfigObject.AdditionalItems["withCredentials"] = true;
                options.ConfigObject.AdditionalItems["showMutatedRequest"] = false;
                
                options.InjectJavascript(
                    "/swagger-ui/csrf-interceptor.js");
                
                options.UseRequestInterceptor(
                    "(request) => { return window.csrfRequestInterceptor(request); }");
            });
        }

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
            HttpOnly = HttpOnlyPolicy.Always,
            Secure = CookieSecurePolicy.Always
        });
        
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.UseMiddleware<CsrfProtectionMiddleware>();
        
        app.MapControllers();
        app.UseHealthCheck();

        return app;
    }
}
