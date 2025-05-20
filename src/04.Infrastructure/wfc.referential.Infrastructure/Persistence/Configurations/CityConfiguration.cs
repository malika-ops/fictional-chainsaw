using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;
public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => new CityId(value));

        builder.Property(c => c.Code)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired();

        builder.Property(c => c.TimeZone)
            .IsRequired();

        builder.Property(c => c.RegionId)
            .HasConversion(
                id => id.Value,
                value => new RegionId(value))
            .IsRequired(); 

        builder.Property(c => c.Abbreviation)
            .IsRequired(false);

        builder.Property(c => c.IsEnabled)
            .IsRequired();

        builder.HasMany(c => c.Sectors)
            .WithOne(e => e.City)
            .HasForeignKey(c => c.CityId)
            .HasPrincipalKey(e => e.Id)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.LastModified)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}

