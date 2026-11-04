namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed record UpdatePropertyResponse(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    string Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string? ImageUrl);
