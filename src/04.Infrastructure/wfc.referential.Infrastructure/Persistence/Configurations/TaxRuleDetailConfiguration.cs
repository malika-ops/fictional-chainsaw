using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class TaxRuleDetailConfiguration : IEntityTypeConfiguration<TaxRuleDetail>
{
    public void Configure(EntityTypeBuilder<TaxRuleDetail> builder)
    {
        builder.HasKey(trd => trd.Id);

        builder.Property(trd => trd.Id)
            .HasConversion(id => id.Value, value => TaxRuleDetailsId.Of(value));

        builder.Property(trd => trd.TaxId).HasConversion(id => id.Value, value => TaxId.Of(value));
        builder.Property(trd => trd.CorridorId).HasConversion(id => id.Value, value => CorridorId.Of(value));
        builder.Property(trd => trd.ServiceId).HasConversion(id => id.Value, value => ServiceId.Of(value));

        builder.Property(trd => trd.IsEnabled)
            .IsRequired().HasDefaultValue(true);

        builder.HasOne(trd => trd.Corridor)
            .WithMany(c => c.TaxRuleDetails)
            .HasForeignKey(trd => trd.CorridorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(trd => trd.Tax)
            .WithMany(tax => tax.TaxRuleDetails)
            .HasForeignKey(trd => trd.TaxId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(trd => trd.Service)
            .WithMany(service => service.TaxRuleDetails)
            .HasForeignKey(trd => trd.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(trd => trd.AppliedOn).
            HasConversion(
              trd => trd.ToString(),
              trd => (ApplicationRule)Enum.Parse(typeof(ApplicationRule), trd)
            );

        builder.Property(trd => trd.IsEnabled).IsRequired();
    }
}
