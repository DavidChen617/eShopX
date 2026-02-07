using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Proxy;

public static class ServiceCollectionProxyExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection DecorateWithDispatchProxy<TInterface, TProxy>()
            where TInterface : class
            where TProxy : DispatchProxyBase<TInterface>
        {
            return services.DecorateWithDispatchProxy(typeof(TInterface), typeof(TProxy));
        }

        public IServiceCollection DecorateWithDispatchProxy(Type interfaceType,
            Type proxyType)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));
            if (proxyType == null) throw new ArgumentNullException(nameof(proxyType));
            if (!interfaceType.IsInterface) throw new ArgumentException("interfaceType must be an interface.", nameof(interfaceType));
            if (proxyType.IsGenericTypeDefinition)
                throw new ArgumentException("proxyType must be a closed generic type.", nameof(proxyType));

            var baseProxyType = typeof(DispatchProxyBase<>).MakeGenericType(interfaceType);
            if (!baseProxyType.IsAssignableFrom(proxyType))
            {
                throw new ArgumentException(
                    $"proxyType must inherit from DispatchProxyBase<{interfaceType.Name}>.", nameof(proxyType));
            }

            DispatchProxyInvoker.Precompile(interfaceType);

            var descriptors = services.Where(s => s.ServiceType == interfaceType).ToList();
            if (descriptors.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Attempted to decorate {interfaceType}, but no such services are present in ServiceCollection");
            }

            var createMethod = typeof(DispatchProxyFactory)
                .GetMethod(nameof(DispatchProxyFactory.Create))!
                .MakeGenericMethod(interfaceType, proxyType);

            foreach (var descriptor in descriptors)
            {
                ServiceDescriptor decorated;
                if (descriptor.IsKeyedService)
                {
                    var key = descriptor.ServiceKey;
                    decorated = ServiceDescriptor.DescribeKeyed(
                        interfaceType,
                        key,
                        (sp, _) =>
                        {
                            var target = CreateInstance(sp, descriptor, key);
                            return createMethod.Invoke(null, new[] { target, sp })!;
                        },
                        descriptor.Lifetime);
                }
                else
                {
                    decorated = ServiceDescriptor.Describe(
                        interfaceType,
                        sp =>
                        {
                            var target = CreateInstance(sp, descriptor);
                            return createMethod.Invoke(null, new[] { target, sp })!;
                        },
                        descriptor.Lifetime);
                }

                services.Remove(descriptor);
                services.Add(decorated);
            }

            return services;
        }

        public IServiceCollection DecorateWithDispatchProxyFromAttributes(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentException("At least one assembly is required.", nameof(assemblies));

            foreach (var iface in assemblies.SelectMany(a => a.DefinedTypes)
                         .Where(t => t.IsInterface && typeof(IInterceptable).IsAssignableFrom(t)))
            {
                var attributes = iface.GetCustomAttributes<UseDispatchProxyAttribute>(inherit: true).ToArray();
                if (attributes.Length == 0) continue;

                foreach (var attribute in attributes)
                {
                    var interfaceType = iface.AsType();
                    var proxyType = attribute.ProxyType;

                    if (interfaceType.ContainsGenericParameters)
                    {
                        var closedInterfaces = services
                            .Select(s => s.ServiceType)
                            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
                            .Distinct()
                            .ToList();

                        if (closedInterfaces.Count == 0)
                        {
                            continue;
                        }

                        foreach (var closedInterface in closedInterfaces)
                        {
                            var closedProxyType = proxyType;
                            if (closedProxyType.IsGenericTypeDefinition)
                            {
                                closedProxyType = closedProxyType.MakeGenericType(closedInterface);
                            }

                            services.DecorateWithDispatchProxy(closedInterface, closedProxyType);
                        }

                        continue;
                    }

                    if (proxyType.IsGenericTypeDefinition)
                    {
                        proxyType = proxyType.MakeGenericType(interfaceType);
                    }

                    services.DecorateWithDispatchProxy(interfaceType, proxyType);
                }
            }

            return services;
        }
    }

    private static object CreateInstance(IServiceProvider services, ServiceDescriptor descriptor, object? key = null)
    {
        if (descriptor.IsKeyedService)
        {
            if (descriptor.KeyedImplementationInstance != null)
            {
                return descriptor.KeyedImplementationInstance;
            }

            if (descriptor.KeyedImplementationFactory != null)
            {
                return descriptor.KeyedImplementationFactory(services, key);
            }

            if (descriptor.KeyedImplementationType != null)
            {
                return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.KeyedImplementationType);
            }
        }

        if (descriptor.ImplementationInstance != null)
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory != null)
        {
            return descriptor.ImplementationFactory(services);
        }

        return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType!);
    }
}
