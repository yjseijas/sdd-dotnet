using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(property => property.Id);

        builder.Property(property => property.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(property => property.Description)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(property => property.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(property => property.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(property => property.Status)
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<PropertyStatus>(value))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(property => property.BedroomCount)
            .IsRequired();

        builder.Property(property => property.BathroomCount)
            .IsRequired();

        builder.Property(property => property.AreaSquareMeters)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(property => property.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(property => property.CreatedAt)
            .IsRequired();

        builder.Property(property => property.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(property => property.Status);
        builder.HasIndex(property => property.Address);
    }
}
