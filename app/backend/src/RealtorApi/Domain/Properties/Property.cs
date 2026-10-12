namespace RealtorApi.Domain.Properties;

public sealed class Property
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public PropertyStatus Status { get; set; }

    public int BedroomCount { get; set; }

    public int BathroomCount { get; set; }

    public decimal AreaSquareMeters { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
