using Microsoft.AspNetCore.Http;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties;

public sealed class PropertiesSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/properties", HandleAsync)
            .WithName("GetProperties")
            .WithSummary("Get a paginated list of properties.")
            .WithDescription("Returns properties ordered by title with pagination metadata and absolute image URLs.")
            .Produces<PaginatedPropertiesResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    public static async Task<IResult> HandleAsync(
        [AsParameters] GetPropertiesRequest request,
        HttpContext httpContext,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var page = request.GetPageOrDefault();
        var pageSize = request.GetPageSizeOrDefault();

        var propertiesResponse = await GetPropertiesQuery.ExecuteAsync(
            dbContext,
            page,
            pageSize,
            httpContext.Request.Host.Value ?? "localhost",
            httpContext.Request.Scheme,
            cancellationToken);

        return Results.Ok(propertiesResponse);
    }
}
