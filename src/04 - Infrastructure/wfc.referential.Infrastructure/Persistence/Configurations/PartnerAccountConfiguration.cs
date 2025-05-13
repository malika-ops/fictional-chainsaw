using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.BankAggregate;

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

        builder.Property(p => p.AccountNumber).IsRequired();
        builder.Property(p => p.RIB).IsRequired();
        builder.Property(p => p.Domiciliation).IsRequired();
        builder.Property(p => p.BusinessName).IsRequired();
        builder.Property(p => p.ShortName).IsRequired();
        builder.Property(p => p.AccountBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(p => p.AccountNumber).IsUnique();
        builder.HasIndex(p => p.RIB).IsUnique();

        builder.Property(p => p.BankId)
            .HasConversion(
                Id => Id.Value,
                value => new BankId(value));

        builder.Property(p => p.AccountType)
            .HasConversion(
                v => v.ToString(),
                v => (AccountType)Enum.Parse(typeof(AccountType), v));

        // Configure la relation avec Bank
        builder.HasOne(p => p.Bank)
            .WithMany()
            .HasForeignKey(p => p.BankId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}