using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.CreateProperty;

namespace RealtorApiTests.Properties.CreateProperty;

public sealed class CreatePropertyMappingTests
{
    [Fact]
    public void ToEntity_MapsRequestAndLeavesImageUrlNull()
    {
        var request = new CreatePropertyRequest
        {
            Title = "Casa central",
            Description = "Casa luminosa",
            Address = "Calle 123",
            Price = 125000m,
            Status = "Available",
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 120m
        };

        var property = request.ToEntity();

        Assert.Equal(request.Title, property.Title);
        Assert.Equal(request.Description, property.Description);
        Assert.Equal(request.Address, property.Address);
        Assert.Equal(request.Price, property.Price);
        Assert.Equal(PropertyStatus.Available, property.Status);
        Assert.Equal(request.BedroomCount, property.BedroomCount);
        Assert.Equal(request.BathroomCount, property.BathroomCount);
        Assert.Equal(request.AreaSquareMeters, property.AreaSquareMeters);
        Assert.Null(property.ImageUrl);
    }

    [Fact]
    public void ToResponse_MapsPropertyAndPreservesNullImageUrl()
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa central",
            Description = "Casa luminosa",
            Address = "Calle 123",
            Price = 125000m,
            Status = PropertyStatus.Available,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 120m,
            ImageUrl = null
        };

        var response = property.ToResponse();

        Assert.Equal(property.Id, response.Id);
        Assert.Equal(property.Title, response.Title);
        Assert.Equal(property.Description, response.Description);
        Assert.Equal(property.Address, response.Address);
        Assert.Equal(property.Price, response.Price);
        Assert.Equal(property.Status.ToString(), response.Status);
        Assert.Equal(property.BedroomCount, response.BedroomCount);
        Assert.Equal(property.BathroomCount, response.BathroomCount);
        Assert.Equal(property.AreaSquareMeters, response.AreaSquareMeters);
        Assert.Null(response.ImageUrl);
    }
}
