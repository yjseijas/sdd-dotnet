using RealtorApi.Features.Properties.UpdateProperty;

namespace RealtorApiTests.Properties.UpdateProperty;

public sealed class UpdatePropertyValidatorTests
{
    [Fact]
    public void Validate_WhenRequestIsValid_Succeeds()
    {
        var request = new UpdatePropertyRequest
        {
            Id = Guid.NewGuid(),
            Title = "Casa moderna",
            Description = "Casa en zona tranquila",
            Address = "Calle 42",
            Price = 125000m,
            Status = "Available",
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 120m
        };

        var result = new UpdatePropertyValidator().Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Rentedx")]
    public void Validate_WhenStatusIsNotAllowed_Fails(string status)
    {
        var request = new UpdatePropertyRequest
        {
            Id = Guid.NewGuid(),
            Title = "Casa moderna",
            Description = "Descripcion",
            Address = "Calle 42",
            Price = 125000m,
            Status = status,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 100m
        };

        var result = new UpdatePropertyValidator().Validate(request);

        Assert.False(result.IsValid);
    }
}
