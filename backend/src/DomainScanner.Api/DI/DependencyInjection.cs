using DomainScanner.Api.Extensions;
using DomainScanner.Api.Middleware;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;

namespace DomainScanner.Api.DI;

public static class DependencyInjection
{
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