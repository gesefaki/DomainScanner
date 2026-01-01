using System.Reflection;
using DomainScanner.Application.Abstractions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Get our assembly
        var assembly = Assembly.GetExecutingAssembly();
        
        // Automatic handlers registration
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IRequestHandler<,>))
                .Where(c => !c.IsAbstract))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        
        return services;
    }

    private static IServiceCollection AddRequestHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            services.AddScoped(handlerInterface, handlerType);
        }

        return services;
    }

    public static IServiceCollection AddRequestHandlers(this IServiceCollection services)
    {
        return services.AddRequestHandlersFromAssembly(Assembly.GetExecutingAssembly());
    }
}