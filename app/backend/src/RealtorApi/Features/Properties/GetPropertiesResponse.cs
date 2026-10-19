namespace RealtorApi.Features.Properties;

public sealed record PropertySummaryResponse(
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

public sealed record PaginatedPropertiesResponse(
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNext,
    bool HasPrevious,
    IReadOnlyList<PropertySummaryResponse> Items);
