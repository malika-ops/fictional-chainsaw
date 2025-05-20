using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;
public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                Id => Id.Value,
                value => new RegionId(value));

        builder.Property(r => r.CountryId)
           .HasConversion(
               id => id.Value,        // Store AreaId as GUID
               value => new CountryId(value)); // Convert back to AreaId

        builder.Property(r => r.Name).IsRequired();

        builder.Property(r => r.Code).IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();

        builder.Property(r => r.IsEnabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasMany(r => r.Cities)
            .WithOne()
            .HasForeignKey(c => c.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.LastModified)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
