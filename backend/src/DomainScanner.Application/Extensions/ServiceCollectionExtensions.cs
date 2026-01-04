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
        
        // Automatic handlers registration from reflection.
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
        // Getting types from assembly
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        // Service registration (Scoped for Mediator)
        foreach (var handlerType in handlerTypes)
        {
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            services.AddScoped(handlerInterface, handlerType);
        }

        return services;
    }

    // Interface for using in Program.cs
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services)
    {
        return services.AddRequestHandlersFromAssembly(Assembly.GetExecutingAssembly());
    }
}