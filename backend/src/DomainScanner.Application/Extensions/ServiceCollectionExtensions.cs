using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.Pipelines.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateDomainCommandHandler>();
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        return services;
    }
}