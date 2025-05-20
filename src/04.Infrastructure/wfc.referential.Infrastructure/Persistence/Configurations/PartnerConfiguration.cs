using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;

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
        builder.Property(p => p.Type).IsRequired();

        builder.Property(p => p.NetworkMode)
            .HasConversion(
                v => v.ToString(),
                v => (NetworkMode)Enum.Parse(typeof(NetworkMode), v));

        builder.Property(p => p.PaymentMode)
            .HasConversion(
                v => v.ToString(),
                v => (PaymentMode)Enum.Parse(typeof(PaymentMode), v));

        builder.Property(p => p.IdParent);

        builder.Property(p => p.SupportAccountType)
            .HasConversion(
                v => v.ToString(),
                v => (SupportAccountType)Enum.Parse(typeof(SupportAccountType), v));

        builder.Property(p => p.TaxIdentificationNumber);
        builder.Property(p => p.TaxRegime);
        builder.Property(p => p.AuxiliaryAccount);
        builder.Property(p => p.ICE);
        builder.Property(p => p.RASRate);

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Logo);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.TaxIdentificationNumber).IsUnique();
        builder.HasIndex(p => p.ICE).IsUnique();

        // Account relationships
        builder.Property(p => p.CommissionAccountId);
        builder.Property(p => p.ActivityAccountId);
        builder.Property(p => p.SupportAccountId);

        // Ignore navigation properties to avoid EF Core trying to map them
        builder.Ignore(p => p.CommissionAccount);
        builder.Ignore(p => p.ActivityAccount);
        builder.Ignore(p => p.SupportAccount);
    }
}