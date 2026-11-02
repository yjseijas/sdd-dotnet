using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Properties.CreateProperty;

public sealed class CreatePropertySliceTests
{
    [Fact]
    public async Task HandleAsync_WhenHandlerSucceeds_Returns201WithCreatedProperty()
    {
        var response = new CreatePropertyResponse(
            Guid.NewGuid(),
            "Casa central",
            "Casa luminosa",
            "Calle 123",
            125000m,
            "Available",
            3,
            2,
            120m,
            null);
        var handler = new StubCreatePropertyHandler(Result<CreatePropertyResponse>.Success(response));
        var result = await CreatePropertySlice.HandleAsync(
            new CreatePropertyRequest(),
            handler,
            CancellationToken.None);
        var context = CreateHttpContext();

        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("Location"));
        var payload = await ReadResponseAsync(context);
        Assert.Contains(response.Id.ToString(), payload, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"imageUrl\":null", payload, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(StatusCodes.Status413PayloadTooLarge, "Payload Too Large")]
    [InlineData(StatusCodes.Status500InternalServerError, "Internal Server Error")]
    public async Task HandleAsync_WhenHandlerFails_ReturnsProblemDetails(
        int statusCode,
        string title)
    {
        var handler = new StubCreatePropertyHandler(
            Result<CreatePropertyResponse>.Failure(statusCode, title, "Mensaje seguro.", "test.error"));
        var result = await CreatePropertySlice.HandleAsync(
            new CreatePropertyRequest(),
            handler,
            CancellationToken.None);
        var context = CreateHttpContext();

        await result.ExecuteAsync(context);

        Assert.Equal(statusCode, context.Response.StatusCode);
        var payload = await ReadResponseAsync(context);
        Assert.Contains(title, payload, StringComparison.Ordinal);
        Assert.Contains("Mensaje seguro.", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RequestBodyLimitMiddleware_AllowsExactlyTheAggregateLimit()
    {
        var body = new byte[CreatePropertyRequestBodyLimitMiddleware.MaxBodyBytes];
        var requestBodyBytesRead = 0;
        var nextInvoked = false;
        var middleware = new CreatePropertyRequestBodyLimitMiddleware(async context =>
        {
            nextInvoked = true;
            using var copy = new MemoryStream();
            await context.Request.Body.CopyToAsync(copy);
            requestBodyBytesRead = checked((int)copy.Length);
        });
        var context = CreateMultipartContext(new MemoryStream(body));

        await middleware.InvokeAsync(context);

        Assert.True(nextInvoked);
        Assert.Equal(body.Length, requestBodyBytesRead);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task RequestBodyLimitMiddleware_RejectsChunkedBodyOverTheAggregateLimit()
    {
        var body = new byte[CreatePropertyRequestBodyLimitMiddleware.MaxBodyBytes + 1];
        var nextInvoked = false;
        var middleware = new CreatePropertyRequestBodyLimitMiddleware(_ =>
        {
            nextInvoked = true;
            return Task.CompletedTask;
        });
        var context = CreateMultipartContext(new MemoryStream(body));

        await middleware.InvokeAsync(context);

        Assert.False(nextInvoked);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        var payload = await ReadResponseAsync(context);
        using var document = JsonDocument.Parse(payload);
        Assert.Equal(
            StatusCodes.Status413PayloadTooLarge,
            document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains("6291456", document.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task RequestBodyLimitMiddleware_RejectsOversizedContentLengthWithoutReadingBody()
    {
        var body = new MemoryStream(new byte[] { 1, 2, 3 });
        var middleware = new CreatePropertyRequestBodyLimitMiddleware(_ => Task.CompletedTask);
        var context = CreateMultipartContext(body);
        context.Request.ContentLength = CreatePropertyRequestBodyLimitMiddleware.MaxBodyBytes + 1;

        await middleware.InvokeAsync(context);

        Assert.Equal(0, body.Position);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
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

    private static DefaultHttpContext CreateMultipartContext(Stream requestBody)
    {
        var context = CreateHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/properties";
        context.Request.ContentType = "multipart/form-data; boundary=test-boundary";
        context.Request.Body = requestBody;
        context.Request.ContentLength = null;
        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private sealed class StubCreatePropertyHandler(Result<CreatePropertyResponse> result)
        : IHandler<CreatePropertyRequest, CreatePropertyResponse>
    {
        public Task<Result<CreatePropertyResponse>> HandleAsync(
            CreatePropertyRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }
    }
}
