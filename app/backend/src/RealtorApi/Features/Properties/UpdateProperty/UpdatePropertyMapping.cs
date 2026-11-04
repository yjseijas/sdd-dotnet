using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.UpdateProperty;

public static class UpdatePropertyMapping
{
    public static void ApplyUpdates(this UpdatePropertyRequest request, Property property)
    {
        property.Title = request.Title!;
        property.Description = request.Description!;
        property.Address = request.Address!;
        property.Price = request.Price!.Value;
        property.Status = Enum.Parse<PropertyStatus>(request.Status!, ignoreCase: false);
        property.BedroomCount = request.BedroomCount!.Value;
        property.BathroomCount = request.BathroomCount!.Value;
        property.AreaSquareMeters = request.AreaSquareMeters!.Value;
        property.UpdatedAt = DateTime.UtcNow;
    }

    public static UpdatePropertyResponse ToResponse(this Property property)
    {
        return new UpdatePropertyResponse(
            property.Id,
            property.Title,
            property.Description,
            property.Address,
            property.Price,
            property.Status.ToString(),
            property.BedroomCount,
            property.BathroomCount,
            property.AreaSquareMeters,
            property.ImageUrl);
    }
}
