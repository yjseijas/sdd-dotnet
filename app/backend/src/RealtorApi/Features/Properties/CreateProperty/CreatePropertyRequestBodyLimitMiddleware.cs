using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyRequestBodyLimitMiddleware(RequestDelegate next)
{
    public const long MaxBodyBytes = 6_291_456;
    private const int ReadBufferSize = 81_920;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsTargetRequest(context.Request))
        {
            await next(context);
            return;
        }

        if (context.Request.ContentLength is > MaxBodyBytes)
        {
            await WritePayloadTooLargeAsync(context);
            return;
        }

        await using var bufferedBody = new MemoryStream(capacity: (int)MaxBodyBytes);
        var buffer = new byte[ReadBufferSize];
        long totalBytesRead = 0;

        while (true)
        {
            var remainingBytesIncludingOverflowByte = MaxBodyBytes + 1 - totalBytesRead;
            var bytesToRead = (int)Math.Min(buffer.Length, remainingBytesIncludingOverflowByte);
            var bytesRead = await context.Request.Body.ReadAsync(
                buffer.AsMemory(0, bytesToRead),
                context.RequestAborted);

            if (bytesRead == 0)
            {
                break;
            }

            totalBytesRead += bytesRead;
            if (totalBytesRead > MaxBodyBytes)
            {
                await WritePayloadTooLargeAsync(context);
                return;
            }

            await bufferedBody.WriteAsync(buffer.AsMemory(0, bytesRead), context.RequestAborted);
        }

        bufferedBody.Position = 0;
        var originalBody = context.Request.Body;
        context.Request.Body = bufferedBody;
        try
        {
            await next(context);
        }
        finally
        {
            context.Request.Body = originalBody;
        }
    }

    private static bool IsTargetRequest(HttpRequest request)
    {
        var isMultipart = request.ContentType?.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true;
        if (!isMultipart)
        {
            return false;
        }

        var path = request.Path.Value ?? string.Empty;
        if (HttpMethods.IsPost(request.Method))
        {
            return string.Equals(path, "/api/properties", StringComparison.OrdinalIgnoreCase);
        }

        if (HttpMethods.IsPut(request.Method))
        {
            return path.StartsWith("/api/properties/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(path, "/api/properties", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static async Task WritePayloadTooLargeAsync(HttpContext context)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status413PayloadTooLarge,
            Title = "Payload Too Large",
            Detail = $"El cuerpo multipart no puede superar {MaxBodyBytes.ToString(CultureInfo.InvariantCulture)} bytes.",
            Instance = context.Request.Path
        };

        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problemDetails,
            JsonOptions,
            context.RequestAborted);
    }
}
