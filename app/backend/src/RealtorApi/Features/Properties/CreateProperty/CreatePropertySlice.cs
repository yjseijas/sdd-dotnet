using Microsoft.AspNetCore.Http;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertySlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/properties", HandleAsync)
            .Accepts<CreatePropertyRequest>("multipart/form-data")
            .WithName("CreateProperty")
            .WithSummary("Create a property.")
            .WithDescription("Creates a property from multipart form data, with an optional image.")
            .Produces<CreatePropertyResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    public static async Task<IResult> HandleAsync(
        CreatePropertyRequest request,
        IHandler<CreatePropertyRequest, CreatePropertyResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return Results.Json(result.Value, statusCode: StatusCodes.Status201Created);
        }

        if (result.Error?.StatusCode == StatusCodes.Status400BadRequest)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["image"] = [result.Error.Detail]
            });
        }

        return Results.Problem(result.ToProblemDetails());
    }
}
