using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Mapping;

public static class MappingExtensions
{
    public static IServiceCollection AddMapping(this IServiceCollection services, params Assembly[] assemblies)
    {
        var profiles = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false } && typeof(Profile).IsAssignableFrom(t))
            .Select(t => (Profile)Activator.CreateInstance(t)!)
            .ToList();

        services.AddSingleton(new MapperConfiguration(profiles));
        services.AddSingleton<IMapper, Mapper>();

        return services;
    }
}
