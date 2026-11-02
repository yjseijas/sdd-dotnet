using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.CreateProperty;

public static class CreatePropertyMapping
{
    public static Property ToEntity(this CreatePropertyRequest request)
    {
        return new Property
        {
            Title = request.Title!,
            Description = request.Description!,
            Address = request.Address!,
            Price = request.Price!.Value,
            Status = Enum.Parse<PropertyStatus>(request.Status!, ignoreCase: false),
            BedroomCount = request.BedroomCount!.Value,
            BathroomCount = request.BathroomCount!.Value,
            AreaSquareMeters = request.AreaSquareMeters!.Value,
            ImageUrl = null
        };
    }

    public static CreatePropertyResponse ToResponse(this Property property)
    {
        return new CreatePropertyResponse(
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
