namespace RealtorApi.Features.Properties.CreateProperty;

/// <summary>Representación de una propiedad recién creada.</summary>
public sealed record CreatePropertyResponse(
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
