using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class AffiliateConfiguration : IEntityTypeConfiguration<Affiliate>
{
    public void Configure(EntityTypeBuilder<Affiliate> builder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(
                Id => Id.Value,
                value => new AffiliateId(value));

        // Required properties
        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(255);

        // Optional properties
        builder.Property(a => a.Abbreviation)
            .HasMaxLength(10);

        builder.Property(a => a.OpeningDate);

        builder.Property(a => a.CancellationDay)
            .HasMaxLength(50);

        builder.Property(a => a.Logo)
            .HasMaxLength(500);

        builder.Property(a => a.ThresholdBilling)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(a => a.AccountingDocumentNumber)
            .HasMaxLength(100);

        builder.Property(a => a.AccountingAccountNumber)
            .HasMaxLength(100);

        builder.Property(a => a.StampDutyMention)
            .HasMaxLength(255);

        builder.Property(a => a.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Country relationship
        builder.Property(a => a.CountryId)
            .HasConversion(
                Id => Id.Value,
                value => new CountryId(value))
            .IsRequired();

        builder.HasOne(a => a.Country)
            .WithMany()
            .HasForeignKey(a => a.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.AffiliateType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Unique indexes
        builder.HasIndex(a => a.Code)
            .IsUnique();

        // Table name
        builder.ToTable("Affiliates");
    }
}