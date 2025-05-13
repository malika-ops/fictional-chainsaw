using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class TierConfiguration : IEntityTypeConfiguration<Tier>
{
    public void Configure(EntityTypeBuilder<Tier> builder)
    {
        builder.ToTable("Tiers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
               .HasConversion(id => id.Value, guid => new TierId(guid));

        builder.Property(t => t.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.HasIndex(t => t.Name)
               .IsUnique();

        builder.Property(t => t.Description)
               .HasMaxLength(500);

        builder.Property(t => t.IsEnabled)
               .HasDefaultValue(true)
               .IsRequired();
    }
}