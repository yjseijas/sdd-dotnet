using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests;

public sealed class PropertyPersistenceTests
{
    [Fact]
    public async Task Property_status_is_persisted_as_text()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Title = "Casa de prueba",
                Description = "Propiedad para validación del modelo.",
                Address = "Avenida de prueba 123",
                Price = 2500m,
                Status = PropertyStatus.Available,
                BedroomCount = 2,
                BathroomCount = 1,
                AreaSquareMeters = 120m,
                ImageUrl = "/images/properties/1.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            context.Properties.Add(property);
            await context.SaveChangesAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT \"Status\" FROM properties WHERE \"Id\" = @id";
            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@id";
            idParameter.Value = property.Id;
            command.Parameters.Add(idParameter);

            var storedStatus = (string?)await command.ExecuteScalarAsync();

            Assert.Equal(nameof(PropertyStatus.Available), storedStatus);
        }
    }

    [Fact]
    public async Task Seeder_uses_public_image_paths_and_is_idempotent()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            await DatabaseSeeder.SeedAsync(context);
            var firstCount = await context.Properties.CountAsync();

            await DatabaseSeeder.SeedAsync(context);
            var secondCount = await context.Properties.CountAsync();

            Assert.True(firstCount > 0);
            Assert.Equal(firstCount, secondCount);

            var firstProperty = await context.Properties
                .OrderBy(property => property.CreatedAt)
                .FirstAsync();

            Assert.Equal("/images/properties/1.png", firstProperty.ImageUrl);
        }
    }
}
