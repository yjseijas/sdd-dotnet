using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApiTests.Infrastructure;

public class SliceDiscoveryTests
{
    [Fact]
    public void DiscoverSliceTypes_FindsConcreteSliceAndOmitsDuplicates()
    {
        var sliceTypes = SliceRegistry.DiscoverSliceTypes(typeof(TestSlice).Assembly);

        Assert.Contains(typeof(TestSlice), sliceTypes);
        Assert.Equal(1, sliceTypes.Count(type => type == typeof(TestSlice)));
    }

    [Fact]
    public void AddSlices_RegistersSliceOnlyOnce()
    {
        var services = new ServiceCollection();
        services.AddSlices(typeof(TestSlice).Assembly);

        using var provider = services.BuildServiceProvider();
        var registeredSlices = provider.GetServices<ISlice>().Where(slice => slice is TestSlice).ToList();

        Assert.Single(registeredSlices);
    }

    private sealed class TestSlice : ISlice
    {
        public void AddEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-slice", () => "ok");
        }
    }
}
