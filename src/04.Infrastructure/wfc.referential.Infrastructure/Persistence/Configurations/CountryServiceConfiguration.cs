using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class CountryServiceConfiguration : IEntityTypeConfiguration<CountryService>
{
    public void Configure(EntityTypeBuilder<CountryService> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new CountryServiceId(value));

        builder.Property(x => x.CountryId)
            .HasConversion(
                id => id.Value,
                value => new CountryId(value))
            .IsRequired();

        builder.Property(x => x.ServiceId)
            .HasConversion(
                id => id.Value,
                value => new ServiceId(value))
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Relations
        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Service>()
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CountryId, x.ServiceId }).IsUnique();
    }
}