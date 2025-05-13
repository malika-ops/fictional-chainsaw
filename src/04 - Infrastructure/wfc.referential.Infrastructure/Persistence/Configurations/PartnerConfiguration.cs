using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                Id => Id.Value,
                value => new PartnerId(value));

        builder.Property(p => p.Code).IsRequired();
        builder.Property(p => p.Label).IsRequired();

        builder.Property(p => p.NetworkMode)
            .HasConversion(
                v => v.ToString(),
                v => (NetworkMode)Enum.Parse(typeof(NetworkMode), v));

        builder.Property(p => p.PaymentMode)
            .HasConversion(
                v => v.ToString(),
                v => (PaymentMode)Enum.Parse(typeof(PaymentMode), v));

        builder.Property(p => p.IdPartner).IsRequired();

        builder.Property(p => p.SupportAccountType)
            .HasConversion(
                v => v.ToString(),
                v => (SupportAccountType)Enum.Parse(typeof(SupportAccountType), v));

        builder.Property(p => p.IdentificationNumber);
        builder.Property(p => p.TaxRegime);
        builder.Property(p => p.AuxiliaryAccount);
        builder.Property(p => p.ICE);

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Logo);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.IdentificationNumber).IsUnique();
        builder.HasIndex(p => p.ICE).IsUnique();

        builder.Property(p => p.SectorId)
            .HasConversion(
                Id => Id.Value,
                value => new SectorId(value));

        builder.Property(p => p.CityId)
            .HasConversion(
                Id => Id.Value,
                value => new CityId(value));

        // Relations
        builder.HasOne(p => p.Sector)
            .WithMany()
            .HasForeignKey(p => p.SectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.City)
            .WithMany()
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}