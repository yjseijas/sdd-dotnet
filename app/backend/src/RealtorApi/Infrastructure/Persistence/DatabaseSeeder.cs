using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void Seed(AppDbContext context)
    {
        var seedManifest = LoadSeedManifest();
        var records = LoadPropertyRecords(seedManifest);

        if (records.Count == 0)
        {
            return;
        }

        foreach (var record in records)
        {
            var status = Enum.TryParse<PropertyStatus>(record.Status, true, out var parsedStatus)
                ? parsedStatus
                : PropertyStatus.Available;

            if (context.Properties.Any(property => property.Id == record.Id))
            {
                continue;
            }

            context.Properties.Add(new Property
            {
                Id = record.Id,
                Title = record.Title,
                Description = record.Description,
                Address = record.Address,
                Price = record.Price,
                Status = status,
                BedroomCount = record.BedroomCount,
                BathroomCount = record.BathroomCount,
                AreaSquareMeters = record.AreaSquareMeters,
                ImageUrl = ResolvePublicImageUrl(seedManifest, record.ImageUrl),
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            });
        }

        context.SaveChanges();
    }

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var seedManifest = LoadSeedManifest();
        var records = LoadPropertyRecords(seedManifest);

        if (records.Count == 0)
        {
            return;
        }

        foreach (var record in records)
        {
            var status = Enum.TryParse<PropertyStatus>(record.Status, true, out var parsedStatus)
                ? parsedStatus
                : PropertyStatus.Available;

            if (await context.Properties.AnyAsync(property => property.Id == record.Id, cancellationToken))
            {
                continue;
            }

            context.Properties.Add(new Property
            {
                Id = record.Id,
                Title = record.Title,
                Description = record.Description,
                Address = record.Address,
                Price = record.Price,
                Status = status,
                BedroomCount = record.BedroomCount,
                BathroomCount = record.BathroomCount,
                AreaSquareMeters = record.AreaSquareMeters,
                ImageUrl = ResolvePublicImageUrl(seedManifest, record.ImageUrl),
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static SeedManifest LoadSeedManifest()
    {
        var path = ResolveSeedFilePath("support/seed-data/seed-manifest.json");
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<SeedManifest>(json, JsonOptions)
            ?? throw new InvalidOperationException("The property seed manifest could not be loaded.");
    }

    private static List<PropertySeedRecord> LoadPropertyRecords(SeedManifest seedManifest)
    {
        var propertiesPath = ResolveSeedFilePath(seedManifest.PropertiesFile);
        var recordsJson = File.ReadAllText(propertiesPath);

        return JsonSerializer.Deserialize<List<PropertySeedRecord>>(recordsJson, JsonOptions)
            ?? new List<PropertySeedRecord>();
    }

    private static string ResolvePublicImageUrl(SeedManifest seedManifest, string rawImageUrl)
    {
        if (string.IsNullOrWhiteSpace(rawImageUrl))
        {
            return string.Empty;
        }

        var fileName = Path.GetFileName(rawImageUrl);

        var match = seedManifest.Assets.FirstOrDefault(asset =>
            string.Equals(Path.GetFileName(asset.Source), fileName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(Path.GetFileName(asset.Target), fileName, StringComparison.OrdinalIgnoreCase));

        if (match is not null && !string.IsNullOrWhiteSpace(match.PublicPath))
        {
            return match.PublicPath;
        }

        return "/images/properties/" + fileName;
    }

    private static string ResolveSeedFilePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new InvalidOperationException("A seed file path is required.");
        }

        var candidateRoots = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var root in candidateRoots)
        {
            var current = new DirectoryInfo(root);
            while (current is not null)
            {
                var candidate = Path.Combine(current.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }
        }

        throw new FileNotFoundException($"The seed file '{relativePath}' could not be found.", relativePath);
    }

    private sealed class SeedManifest
    {
        public string Version { get; set; } = string.Empty;

        public string PropertiesFile { get; set; } = "support/seed-data/properties.json";

        public string StatusFile { get; set; } = "support/seed-data/properties.statuses.json";

        public List<SeedAsset> Assets { get; set; } = new();
    }

    private sealed class SeedAsset
    {
        public string Source { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public string PublicPath { get; set; } = string.Empty;
    }

    private sealed class PropertySeedRecord
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Status { get; set; } = string.Empty;

        public int BedroomCount { get; set; }

        public int BathroomCount { get; set; }

        public decimal AreaSquareMeters { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
