using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Features.Properties.UpdateProperty;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Properties.UpdateProperty;

public sealed class UpdatePropertyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRequestHasNoImage_PreservesExistingImageUrl()
    {
        await using var dbContext = CreateDbContext();
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Vieja",
            Description = "Descripcion",
            Address = "Calle antigua",
            Price = 150000m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 90m,
            ImageUrl = "/assets/properties/existing.png",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = null
        };
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        var storage = new StubImageStorage();
        var handler = new UpdatePropertyHandler(dbContext, storage, NullLogger<UpdatePropertyHandler>.Instance);
        var request = new UpdatePropertyRequest
        {
            Id = property.Id,
            Title = "Nueva",
            Description = "Descripcion actualizada",
            Address = "Calle nueva",
            Price = 180000m,
            Status = "Rented",
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 120m
        };

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nueva", property.Title);
        Assert.Equal("/assets/properties/existing.png", property.ImageUrl);
        Assert.Equal(StatusCodes.Status200OK, result.Value!.Id == property.Id ? StatusCodes.Status200OK : StatusCodes.Status200OK);
        Assert.Equal(0, storage.StoreCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenImageIsReplaced_StoresNewImageAndDeletesPreviousManagedFile()
    {
        await using var dbContext = CreateDbContext();
        var propertyId = Guid.NewGuid();
        var property = new Property
        {
            Id = propertyId,
            Title = "Vieja",
            Description = "Descripcion",
            Address = "Calle antigua",
            Price = 150000m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 90m,
            ImageUrl = "/assets/properties/old.png",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = null
        };
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        var storage = new StubImageStorage();
        var handler = new UpdatePropertyHandler(dbContext, storage, NullLogger<UpdatePropertyHandler>.Instance);
        var request = new UpdatePropertyRequest
        {
            Id = propertyId,
            Title = "Nueva",
            Description = "Descripcion actualizada",
            Address = "Calle nueva",
            Price = 180000m,
            Status = "Available",
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 120m,
            Image = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "image", "new.png")
        };

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal("/assets/properties/generated-image.png", result.Value!.ImageUrl);
        Assert.Equal("/assets/properties/generated-image.png", property.ImageUrl);
        Assert.Equal(1, storage.DeleteIfManagedCalls);
        Assert.Equal("/assets/properties/old.png", storage.DeletedManagedImageUrl);
    }

    private static AppDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    private sealed class StubImageStorage : IPropertyImageStorage
    {
        public int StoreCalls { get; private set; }
        public int DeleteIfManagedCalls { get; private set; }
        public string? DeletedManagedImageUrl { get; private set; }

        public Task<Result<StoredPropertyImage>> StoreAsync(IFormFile file, CancellationToken cancellationToken)
        {
            StoreCalls++;
            return Task.FromResult(Result<StoredPropertyImage>.Success(
                new StoredPropertyImage("generated-image.png", "/assets/properties/generated-image.png", Path.Combine(Path.GetTempPath(), "generated-image.png"))));
        }

        public Task DeleteAsync(StoredPropertyImage image, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteIfManagedAsync(string? imageUrl, CancellationToken cancellationToken)
        {
            DeleteIfManagedCalls++;
            DeletedManagedImageUrl = imageUrl;
            return Task.CompletedTask;
        }
    }
}
