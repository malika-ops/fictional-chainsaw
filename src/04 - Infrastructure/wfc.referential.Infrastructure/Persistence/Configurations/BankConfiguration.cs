using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.BankAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(
                Id => Id.Value,
                value => new BankId(value));

        builder.Property(b => b.Code).IsRequired();
        builder.Property(b => b.Name).IsRequired();
        builder.Property(b => b.Abbreviation).IsRequired();

        builder.HasIndex(b => b.Code).IsUnique();

        builder.Property(b => b.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
    }
}