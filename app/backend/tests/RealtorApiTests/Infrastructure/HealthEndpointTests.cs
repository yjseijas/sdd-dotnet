using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RealtorApiTests.Infrastructure;

public class HealthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public HealthEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyStatus()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"healthy\"", content, StringComparison.Ordinal);
    }
}

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Data Source=RealtorApiTest.db"
            });
        });
    }
}
