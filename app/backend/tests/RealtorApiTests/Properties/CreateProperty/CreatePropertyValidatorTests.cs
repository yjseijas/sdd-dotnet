using RealtorApi.Features.Properties.CreateProperty;

namespace RealtorApiTests.Properties.CreateProperty;

public sealed class CreatePropertyValidatorTests
{
    private readonly CreatePropertyValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ReturnsNoErrors()
    {
        var result = _validator.Validate(CreateValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithRequiredTextMissing_ReturnsErrorsForThoseFields()
    {
        var request = CreateValidRequest() with
        {
            Title = " ",
            Description = "",
            Address = null
        };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Title) && !string.IsNullOrWhiteSpace(error.ErrorMessage));
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Description) && !string.IsNullOrWhiteSpace(error.ErrorMessage));
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Address) && !string.IsNullOrWhiteSpace(error.ErrorMessage));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositivePriceOrArea_ReturnsErrors(decimal value)
    {
        var priceResult = _validator.Validate(CreateValidRequest() with { Price = value });
        var areaResult = _validator.Validate(CreateValidRequest() with { AreaSquareMeters = value });

        Assert.Contains(priceResult.Errors, error =>
            error.PropertyName == nameof(CreatePropertyRequest.Price) && !string.IsNullOrWhiteSpace(error.ErrorMessage));
        Assert.Contains(areaResult.Errors, error =>
            error.PropertyName == nameof(CreatePropertyRequest.AreaSquareMeters)
            && !string.IsNullOrWhiteSpace(error.ErrorMessage));
    }

    [Fact]
    public void Validate_WithNegativeRoomCounts_ReturnsErrors()
    {
        var result = _validator.Validate(CreateValidRequest() with
        {
            BedroomCount = -1,
            BathroomCount = -1
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(CreatePropertyRequest.BedroomCount) && !string.IsNullOrWhiteSpace(error.ErrorMessage));
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(CreatePropertyRequest.BathroomCount) && !string.IsNullOrWhiteSpace(error.ErrorMessage));
    }

    [Fact]
    public void Validate_WithMissingNumericValues_ReturnsErrors()
    {
        var result = _validator.Validate(CreateValidRequest() with
        {
            Price = null,
            BedroomCount = null,
            BathroomCount = null,
            AreaSquareMeters = null
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreatePropertyRequest.Price));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreatePropertyRequest.BedroomCount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreatePropertyRequest.BathroomCount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreatePropertyRequest.AreaSquareMeters));
    }

    [Theory]
    [InlineData("Available")]
    [InlineData("Rented")]
    [InlineData("Maintenance")]
    public void Validate_WithSupportedStatus_IsValid(string status)
    {
        var result = _validator.Validate(CreateValidRequest() with { Status = status });

        Assert.DoesNotContain(result.Errors, error => error.PropertyName == nameof(CreatePropertyRequest.Status));
    }

    [Theory]
    [InlineData("")]
    [InlineData("available")]
    [InlineData("Sold")]
    public void Validate_WithUnsupportedStatus_ReturnsStatusError(string status)
    {
        var result = _validator.Validate(CreateValidRequest() with { Status = status });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreatePropertyRequest.Status));
    }

    private static CreatePropertyRequest CreateValidRequest()
    {
        return new CreatePropertyRequest
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
    }
}
