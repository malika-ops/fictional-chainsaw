using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class SupportAccountConfiguration : IEntityTypeConfiguration<SupportAccount>
{
    public void Configure(EntityTypeBuilder<SupportAccount> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                Id => Id.Value,
                value => new SupportAccountId(value));

        builder.Property(s => s.Code).IsRequired();
        builder.Property(s => s.Description).IsRequired();
        builder.Property(s => s.Threshold)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(s => s.Limit)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(s => s.AccountBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(s => s.AccountingNumber).IsRequired();

        builder.Property(s => s.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => s.AccountingNumber).IsUnique();

        builder.Property(s => s.PartnerId)
            .HasConversion(
                Id => Id!.Value,
                value => new PartnerId(value))
            .IsRequired(false);

        builder.Property(s => s.SupportAccountTypeId)
            .HasConversion(
                Id => Id!.Value,
                value => new ParamTypeId(value))
            .IsRequired(false);

        builder.HasOne(s => s.Partner)
            .WithMany()
            .HasForeignKey(s => s.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SupportAccountType)
            .WithMany()
            .HasForeignKey(s => s.SupportAccountTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}