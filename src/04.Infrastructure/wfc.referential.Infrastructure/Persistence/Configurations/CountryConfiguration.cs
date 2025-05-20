using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;
public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                        Id => Id.Value,
                        dbId => new CountryId(dbId));

        builder.Property(o => o.ISO2)
            .IsRequired()
            .HasMaxLength(2)
            .IsFixedLength();

        builder.Property(o => o.ISO3)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength();

        builder.HasMany(p => p.Regions)
                   .WithOne()
                   .HasForeignKey(r => r.CountryId)
                   .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.Code).IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();


        builder.Property(t => t.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
