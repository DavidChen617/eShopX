using System.Reflection;

using eShopX.Common.Mediator.Behaviors;
using eShopX.Common.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Mediator;

public static class MediatorExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<ISender, Sender>();

        // Register handlers
        var handlerInterfaceTypes = new[]
        {
            typeof(IRequestHandler<,>),
            typeof(IRequestHandler<>)
        };

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && handlerInterfaceTypes.Contains(i.GetGenericTypeDefinition())));

            foreach (var handlerType in handlerTypes)
            {
                var implementedInterfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && handlerInterfaceTypes.Contains(i.GetGenericTypeDefinition()));

                foreach (var implementedInterface in implementedInterfaces)
                {
                    services.AddScoped(implementedInterface, handlerType);
                }
            }
        }

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services, params Assembly[] assemblies)
    {
        var validatorInterfaceType = typeof(IValidator<>);

        foreach (var assembly in assemblies)
        {
            var validatorTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorInterfaceType));

            foreach (var validatorType in validatorTypes)
            {
                var implementedInterfaces = validatorType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorInterfaceType);

                foreach (var implementedInterface in implementedInterfaces)
                {
                    services.AddScoped(implementedInterface, validatorType);
                }
            }
        }

        return services;
    }

    public static IServiceCollection AddValidationBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }

    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }
}
