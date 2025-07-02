using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class ContractDetailsConfiguration : IEntityTypeConfiguration<ContractDetails>
{
    public void Configure(EntityTypeBuilder<ContractDetails> builder)
    {
        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Id)
            .HasConversion(
                id => id.Value,
                value => new ContractDetailsId(value));

        builder.Property(cd => cd.ContractId)
            .HasConversion(
                id => id.Value,
                value => new ContractId(value))
            .IsRequired();

        builder.Property(cd => cd.PricingId)
            .HasConversion(
                id => id.Value,
                value => new PricingId(value))
            .IsRequired();

        builder.Property(cd => cd.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign key relationships
        builder.HasOne(cd => cd.Contract)
            .WithMany()
            .HasForeignKey(cd => cd.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cd => cd.Pricing)
            .WithMany()
            .HasForeignKey(cd => cd.PricingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on ContractId + PricingId combination
        builder.HasIndex(cd => new { cd.ContractId, cd.PricingId })
            .IsUnique()
            .HasDatabaseName("IX_ContractDetails_ContractId_PricingId");

        // Table name
        builder.ToTable("ContractDetails");
    }
}