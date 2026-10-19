using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RealtorApiTests.Properties;

public sealed class PropertiesEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public PropertiesEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProperties_ReturnsFirstPageWithMetadataAndAbsoluteImageUrls()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/properties");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GetPropertiesResponseContract>();

        Assert.NotNull(payload);
        Assert.Equal(1, payload!.Page);
        Assert.Equal(6, payload.PageSize);
        Assert.True(payload.TotalItems > 0);
        Assert.True(payload.TotalPages >= 1);
        Assert.InRange(payload.Items.Count, 1, payload.PageSize);
        Assert.Equal(Math.Min(payload.TotalItems, payload.PageSize), payload.Items.Count);
        Assert.All(payload.Items, item =>
        {
            Assert.True(Uri.TryCreate(item.ImageUrl, UriKind.Absolute, out var imageUri));
            Assert.Equal("http", imageUri!.Scheme);
            Assert.Equal("localhost", imageUri.Host);
            Assert.StartsWith("/assets/properties/", imageUri.AbsolutePath, StringComparison.Ordinal);
        });

        using var imageResponse = await client.GetAsync(new Uri(payload.Items[0].ImageUrl).PathAndQuery);
        Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
    }

    [Fact]
    public async Task GetProperties_WithPage2AndPageSize6_ReturnsExpectedPaginationMetadata()
    {
        using var client = _factory.CreateClient();

        var firstPageResponse = await client.GetAsync("/api/properties?page=1&pageSize=6");
        firstPageResponse.EnsureSuccessStatusCode();
        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<GetPropertiesResponseContract>();

        var response = await client.GetAsync("/api/properties?page=2&pageSize=6");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GetPropertiesResponseContract>();

        Assert.NotNull(firstPage);
        Assert.NotNull(payload);
        Assert.Equal(6, firstPage!.Items.Count);
        Assert.Equal(2, payload!.Page);
        Assert.Equal(6, payload.PageSize);
        Assert.True(payload.TotalPages >= 2 || payload.Items.Count == 0);
        Assert.True(payload.HasPrevious);
        Assert.Equal(payload.Page < payload.TotalPages, payload.HasNext);

        var combinedItems = firstPage.Items.Concat(payload.Items).ToArray();
        var sortedItems = combinedItems
            .OrderBy(item => item.Title, StringComparer.Ordinal)
            .ThenBy(item => item.Id)
            .ToArray();
        Assert.Equal(sortedItems.Select(item => item.Id), combinedItems.Select(item => item.Id));
    }

    [Theory]
    [InlineData("?page=abc", "Page")]
    [InlineData("?page=0", "Page")]
    [InlineData("?page=-1", "Page")]
    [InlineData("?pageSize=abc", "PageSize")]
    [InlineData("?pageSize=0", "PageSize")]
    [InlineData("?pageSize=101", "PageSize")]
    public async Task GetProperties_WithInvalidPaginationValues_ReturnsValidationProblem(
        string query,
        string expectedField)
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/properties" + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
        Assert.Contains(problem.Errors.Keys, key => string.Equals(key, expectedField, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetProperties_WhenPageExceedsRange_ReturnsEmptyCollectionAndMetadata()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/properties?page=2147483647&pageSize=100");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GetPropertiesResponseContract>();

        Assert.NotNull(payload);
        Assert.Equal(int.MaxValue, payload!.Page);
        Assert.Empty(payload.Items);
        Assert.False(payload.HasNext);
        Assert.True(payload.HasPrevious);
        Assert.Equal(payload.Page > 1, payload.HasPrevious);
    }

    private sealed record GetPropertiesResponseContract(
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages,
        bool HasNext,
        bool HasPrevious,
        IReadOnlyList<PropertyItemContract> Items);

    private sealed record PropertyItemContract(
        Guid Id,
        string Title,
        string Description,
        string Address,
        decimal Price,
        string Status,
        int BedroomCount,
        int BathroomCount,
        decimal AreaSquareMeters,
        string ImageUrl);
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
