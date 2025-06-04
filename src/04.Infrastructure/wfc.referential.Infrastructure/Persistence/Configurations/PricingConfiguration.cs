using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class PricingConfiguration : IEntityTypeConfiguration<Pricing>
{
    public void Configure(EntityTypeBuilder<Pricing> builder)
    {
        builder.ToTable("Pricings");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .HasConversion(id => id.Value, guid => new PricingId(guid));

        builder.Property(p => p.Code)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(p => p.Channel)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(p => p.MinimumAmount)
               .IsRequired()
               .HasPrecision(18, 2);

        builder.Property(p => p.MaximumAmount)
               .IsRequired()
               .HasPrecision(18, 2);

        builder.Property(p => p.FixedAmount)
               .HasPrecision(18, 2);

        builder.Property(p => p.Rate)
               .HasPrecision(9, 4);           // e.g., 0.1500 = 15 %

        builder.Property(p => p.IsEnabled)
               .HasDefaultValue(true)
               .IsRequired();

        builder.HasIndex(p => p.Code)
               .IsUnique();

        builder.HasOne(p => p.Service)
               .WithMany()
               .HasForeignKey(p => p.ServiceId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Corridor)
               .WithMany()
               .HasForeignKey(p => p.CorridorId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Affiliate)
               .WithMany()
               .HasForeignKey(p => p.AffiliateId)
               .OnDelete(DeleteBehavior.ClientSetNull);
    }
}