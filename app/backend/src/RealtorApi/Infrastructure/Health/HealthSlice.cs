using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApi.Infrastructure.Health;

public sealed class HealthSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Microsoft.AspNetCore.Http.Results.Ok(new { status = "healthy" }))
            .WithName("Health")
            .WithSummary("Check whether the API is healthy.")
            .WithDescription("Returns a lightweight health response without requiring application authentication.")
            .Produces(StatusCodes.Status200OK);
    }
}
