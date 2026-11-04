using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed class UpdatePropertySlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/properties/{id}", HandleAsync)
            .Accepts<UpdatePropertyRequest>("multipart/form-data")
            .WithName("UpdateProperty")
            .WithSummary("Update an existing property.")
            .WithDescription("Updates the editable property fields and optionally replaces the current image.")
            .Produces<UpdatePropertyResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status415UnsupportedMediaType)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    public static async Task<IResult> HandleAsync(
        string id,
        UpdatePropertyRequest request,
        IHandler<UpdatePropertyRequest, UpdatePropertyResponse> handler,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var propertyId))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["id"] = ["El identificador debe ser un GUID válido."]
            });
        }

        var result = await handler.HandleAsync(request with { Id = propertyId }, cancellationToken);
        if (result.IsSuccess)
        {
            return Results.Json(result.Value, statusCode: StatusCodes.Status200OK);
        }

        if (result.Error is not null && result.Error.StatusCode == StatusCodes.Status400BadRequest)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = [result.Error.Detail]
            });
        }

        return Results.Problem(result.ToProblemDetails());
    }
}
