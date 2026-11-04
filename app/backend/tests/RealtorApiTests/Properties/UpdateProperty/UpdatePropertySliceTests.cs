using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Features.Properties.UpdateProperty;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Properties.UpdateProperty;

public sealed class UpdatePropertySliceTests
{
    [Fact]
    public async Task HandleAsync_WhenGuidIsInvalid_ReturnsValidationProblem()
    {
        var handler = new StubUpdatePropertyHandler(Result<UpdatePropertyResponse>.Success(new UpdatePropertyResponse(
            Guid.NewGuid(),
            "Casa",
            "Desc",
            "Calle 1",
            1000m,
            "Available",
            2,
            1,
            60m,
            null)));

        var result = await UpdatePropertySlice.HandleAsync(
            "invalid-guid",
            new UpdatePropertyRequest(),
            handler,
            CancellationToken.None);
        var context = CreateHttpContext();

        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var payload = await ReadResponseAsync(context);
        Assert.Contains("GUID", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenHandlerSucceeds_Returns200WithUpdatedProperty()
    {
        var response = new UpdatePropertyResponse(
            Guid.NewGuid(),
            "Casa actualizada",
            "Descripcion",
            "Calle 5",
            200000m,
            "Available",
            4,
            2,
            150m,
            "/assets/properties/updated.png");
        var handler = new StubUpdatePropertyHandler(Result<UpdatePropertyResponse>.Success(response));

        var result = await UpdatePropertySlice.HandleAsync(
            Guid.NewGuid().ToString(),
            new UpdatePropertyRequest { Title = "Casa actualizada" },
            handler,
            CancellationToken.None);
        var context = CreateHttpContext();

        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        var payload = await ReadResponseAsync(context);
        Assert.Contains(response.Title, payload, StringComparison.Ordinal);
        Assert.Contains(response.ImageUrl!, payload, StringComparison.Ordinal);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.RequestServices = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private sealed class StubUpdatePropertyHandler(Result<UpdatePropertyResponse> result)
        : IHandler<UpdatePropertyRequest, UpdatePropertyResponse>
    {
        public Task<Result<UpdatePropertyResponse>> HandleAsync(
            UpdatePropertyRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }
    }
}
