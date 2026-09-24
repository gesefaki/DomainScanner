using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Application.Handlers.Users.Commands.LoginUser;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Application.Mapping;
using DomainScanner.Application.Pipelines.Behaviors;
using DomainScanner.Application.Services.Auth;
using DomainScanner.Application.Services.Domains;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Application.DI;

/// <summary>
/// Provides extension methods for configuring DI in the application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application-layer services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // Validation
        services.AddScoped<IValidator<CreateDomainCommand>, CreateDomainCommandValidator>();
        services.AddScoped<IValidator<UpdateDomainCommand>, UpdateDomainCommandValidator>();
        services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
        services.AddScoped<IValidator<LoginUserCommand>, LoginUserCommandValidator>();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateDomainCommandHandler>();

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuthenticationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        // Mapping
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        services.AddScoped<IOwnedDomainProvider, OwnedDomainProvider>();

        services.AddScoped<IDomainCheckExecutor, DomainCheckExecutor>();

        return services;
    }

    /// <summary>
    /// Registers the application-layer services required by the background worker.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddWorkerApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.TypeEvaluator = type =>
                type == typeof(HttpSendAndSaveCommandHandler);

            cfg.RegisterServicesFromAssemblyContaining<HttpSendAndSaveCommandHandler>();

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        return services;
    }
}
