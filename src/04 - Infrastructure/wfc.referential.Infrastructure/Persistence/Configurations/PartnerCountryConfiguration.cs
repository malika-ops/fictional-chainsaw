using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class PartnerCountryConfiguration : IEntityTypeConfiguration<PartnerCountry>
{
    public void Configure(EntityTypeBuilder<PartnerCountry> builder)
    {
        builder.ToTable("PartnerCountries");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Id)
               .HasConversion(
                   id => id.Value,
                   db => new PartnerCountryId(db));

        builder.Property(pc => pc.PartnerId)
               .IsRequired()
               .HasConversion(
                   id => id.Value,
                   db => new PartnerId(db));

        builder.Property(pc => pc.CountryId)
               .IsRequired()
               .HasConversion(
                   id => id.Value,
                   db => new CountryId(db));

        builder.HasOne(pc => pc.Partner)
               .WithMany()               
               .HasForeignKey(pc => pc.PartnerId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(pc => pc.Country)
               .WithMany()                 
               .HasForeignKey(pc => pc.CountryId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Property(t => t.IsEnabled)
                .IsRequired()
                .HasDefaultValue(true);

        builder.HasIndex(pc => new { pc.PartnerId, pc.CountryId })
               .IsUnique();
    }
}
