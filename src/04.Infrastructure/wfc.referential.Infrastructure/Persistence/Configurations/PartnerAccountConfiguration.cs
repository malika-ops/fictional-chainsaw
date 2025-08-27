using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class PartnerAccountConfiguration : IEntityTypeConfiguration<PartnerAccount>
{
    public void Configure(EntityTypeBuilder<PartnerAccount> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                Id => Id.Value,
                value => new PartnerAccountId(value));

        // Required fields
        builder.Property(p => p.AccountNumber).IsRequired();
        builder.Property(p => p.RIB).IsRequired();

        // Optional fields
        builder.Property(p => p.Domiciliation).IsRequired(false);
        builder.Property(p => p.BusinessName).IsRequired(false);
        builder.Property(p => p.ShortName).IsRequired(false);

        builder.Property(p => p.AccountBalance).IsRequired();

        builder.Property(p => p.BankId)
            .HasConversion(
                Id => Id.Value,
                value => new BankId(value))
            .IsRequired();

        builder.Property(a => a.PartnerAccountType)
           .HasConversion<string>()
           .HasMaxLength(50)
           .IsRequired();

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(p => p.Bank)
            .WithMany()
            .HasForeignKey(p => p.BankId);

        // Indexes for performance and business rules
        builder.HasIndex(p => p.AccountNumber).IsUnique();
        builder.HasIndex(p => p.RIB).IsUnique();
    }
}