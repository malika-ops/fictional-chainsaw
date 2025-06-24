using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                Id => Id.Value,
                value => new ContractId(value));

        builder.Property(c => c.Code)
            .IsRequired();

        builder.Property(c => c.PartnerId)
            .HasConversion(
                Id => Id.Value,
                value => new PartnerId(value))
            .IsRequired();

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        builder.Property(c => c.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign key relationship to Partner
        builder.HasOne(c => c.Partner)
            .WithMany()
            .HasForeignKey(c => c.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique indexes
        builder.HasIndex(c => c.Code).IsUnique();

        // Table name
        builder.ToTable("Contracts");
    }
}
