using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RealtorApi.Infrastructure.Handlers;

public static class HandlerRegistry
{
    public static IServiceCollection AddHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var assembly in assemblies.Where(assembly => assembly is not null).Distinct())
        {
            foreach (var type in DiscoverHandlerTypes(assembly))
            {
                foreach (var contract in type.GetInterfaces().Where(IsHandlerContract))
                {
                    services.TryAddEnumerable(ServiceDescriptor.Transient(contract, type));
                }
            }
        }

        return services;
    }

    public static IReadOnlyCollection<Type> DiscoverHandlerTypes(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(IHandler).IsAssignableFrom(type)
                && type != typeof(IHandler))
            .Distinct()
            .ToArray();
    }

    private static bool IsHandlerContract(Type interfaceType)
    {
        return interfaceType != typeof(IHandler)
            && typeof(IHandler).IsAssignableFrom(interfaceType);
    }
}
