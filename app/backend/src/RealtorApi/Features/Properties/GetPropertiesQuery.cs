using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties;

public static class GetPropertiesQuery
{
    public static async Task<PaginatedPropertiesResponse> ExecuteAsync(
        AppDbContext dbContext,
        int page,
        int pageSize,
        string requestHost,
        string requestScheme,
        CancellationToken cancellationToken)
    {
        var totalItems = await dbContext.Properties.CountAsync(cancellationToken);

        var offset = ((long)page - 1) * pageSize;
        IReadOnlyList<PropertySummaryResponse> items = offset > int.MaxValue
            ? Array.Empty<PropertySummaryResponse>()
            : await dbContext.Properties
                .AsNoTracking()
                .OrderBy(property => property.Title)
                .ThenBy(property => property.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .Select(property => new PropertySummaryResponse(
                    property.Id,
                    property.Title,
                    property.Description,
                    property.Address,
                    property.Price,
                    property.Status.ToString(),
                    property.BedroomCount,
                    property.BathroomCount,
                    property.AreaSquareMeters,
                    BuildAbsoluteImageUrl(property.ImageUrl, requestHost, requestScheme)))
                .ToListAsync(cancellationToken);

        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PaginatedPropertiesResponse(
            Page: page,
            PageSize: pageSize,
            TotalItems: totalItems,
            TotalPages: totalPages,
            HasNext: page < totalPages,
            HasPrevious: page > 1,
            Items: items);
    }

    private static string BuildAbsoluteImageUrl(string? imageUrl, string requestHost, string requestScheme)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return string.Empty;
        }

        var fileName = Path.GetFileName(imageUrl.Trim());
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var publicPath = "/assets/properties/" + fileName;
        return new Uri(new Uri($"{requestScheme}://{requestHost}"), publicPath).ToString();
    }
}
