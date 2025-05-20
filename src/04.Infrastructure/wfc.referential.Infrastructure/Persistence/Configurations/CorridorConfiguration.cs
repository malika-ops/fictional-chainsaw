using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class CorridorConfiguration : IEntityTypeConfiguration<Corridor>
{
    public void Configure(EntityTypeBuilder<Corridor> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => CorridorId.Of(value));

        builder.Property(x => x.SourceCountryId).HasConversion(id => id.Value, value => CountryId.Of(value));
        builder.Property(x => x.DestinationCountryId).HasConversion(id => id.Value, value => CountryId.Of(value));

        builder.Property(x => x.SourceCityId).HasConversion(id => id.Value, value => CityId.Of(value));
        builder.Property(x => x.DestinationCityId).HasConversion(id => id.Value, value => CityId.Of(value));

        builder.Property(x => x.SourceAgencyId).HasConversion(id => id.Value, value => AgencyId.Of(value));
        builder.Property(x => x.DestinationAgencyId).HasConversion(id => id.Value, value => AgencyId.Of(value));

        builder.Property(x => x.IsEnabled).IsRequired().HasDefaultValue(true);

        builder.HasOne(x => x.SourceCountry)
            .WithMany()
            .HasForeignKey(x => x.SourceCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationCountry)
            .WithMany()
            .HasForeignKey(x => x.DestinationCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceCity)
            .WithMany()
            .HasForeignKey(x => x.SourceCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationCity)
            .WithMany()
            .HasForeignKey(x => x.DestinationCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceAgency)
            .WithMany()
            .HasForeignKey(x => x.SourceAgencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationAgency)
            .WithMany()
            .HasForeignKey(x => x.DestinationAgencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
