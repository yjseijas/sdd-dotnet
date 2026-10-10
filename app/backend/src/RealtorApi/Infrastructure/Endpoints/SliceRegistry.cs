using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RealtorApi.Infrastructure.Endpoints;

public static class SliceRegistry
{
    public static IServiceCollection AddSlices(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var assembly in assemblies.Where(assembly => assembly is not null).Distinct())
        {
            foreach (var type in DiscoverSliceTypes(assembly))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ISlice), type));
            }
        }

        return services;
    }

    public static IReadOnlyCollection<Type> DiscoverSliceTypes(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ISlice).IsAssignableFrom(type))
            .Distinct()
            .ToArray();
    }

    public static IEndpointRouteBuilder MapSlices(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        foreach (var slice in app.ServiceProvider.GetServices<ISlice>().DistinctBy(slice => slice.GetType()))
        {
            slice.AddEndpoint(app);
        }

        return app;
    }
}
