using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class CurrencyDenominationConfiguration : IEntityTypeConfiguration<CurrencyDenomination>
{
    public void Configure(EntityTypeBuilder<CurrencyDenomination> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => new CurrencyDenominationId(value));

        builder.Property(c => c.CurrencyId).IsRequired();
        builder.Property(c => c.Type).IsRequired();
        builder.Property(c => c.Value).IsRequired();

        builder.Property(c => c.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Add unique constraints
        builder.HasIndex(c => c.CurrencyId).IsUnique();
        builder.HasIndex(c => c.Type).IsUnique();
        builder.HasIndex(c => c.Value).IsUnique();
    }
}