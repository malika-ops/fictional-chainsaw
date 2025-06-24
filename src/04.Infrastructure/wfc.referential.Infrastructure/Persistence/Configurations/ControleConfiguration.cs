using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.ControleAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class ControleConfiguration : IEntityTypeConfiguration<Controle>
{
    public void Configure(EntityTypeBuilder<Controle> builder)
    {
        builder.ToTable("Controles");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .HasConversion(id => id.Value,
                              guid => new ControleId(guid));

        builder.Property(c => c.Code)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.IsEnabled)
               .HasDefaultValue(true)
               .IsRequired();

        builder.HasIndex(c => c.Code)
               .IsUnique();
    }
}