using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                Id => Id.Value,
                value => new SectorId(value));

        builder.Property(s => s.Code).IsRequired();
        builder.Property(s => s.Name).IsRequired();

        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
    }
}