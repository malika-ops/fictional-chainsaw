using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => new CurrencyId(value));

        builder.HasMany(c => c.Countries)
            .WithOne(country => country.Currency)
            .HasForeignKey(country => country.CurrencyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.Code).IsRequired();
        builder.Property(c => c.CodeAR).IsRequired();
        builder.Property(c => c.CodeEN).IsRequired();
        builder.Property(c => c.Name).IsRequired();
        builder.Property(c => c.CodeIso).IsRequired();

        builder.Property(c => c.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Add unique constraints
        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.CodeIso).IsUnique();
    }
}