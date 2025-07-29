using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.CurrencyAggregate;
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

        builder.Property(x => x.CurrencyId)
           .HasConversion(
               id => id.Value,
               value => new CurrencyId(value))
           .IsRequired();


        builder.Property(cd => cd.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(cd => cd.Value)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(cd => cd.IsEnabled)
            .IsRequired();
    }
}