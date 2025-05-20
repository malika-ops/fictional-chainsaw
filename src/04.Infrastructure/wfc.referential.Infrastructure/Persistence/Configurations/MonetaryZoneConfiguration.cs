using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.MonetaryZoneAggregate;


namespace wfc.referential.Infrastructure.Persistence.Configurations;
public class MonetaryZoneConfiguration : IEntityTypeConfiguration<MonetaryZone>
{
    public void Configure(EntityTypeBuilder<MonetaryZone> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                        Id => Id.Value,
                        value => new MonetaryZoneId(value));

        builder.HasMany(p => p.Countries)
                   .WithOne()
                   .HasForeignKey(r => r.MonetaryZoneId)
                   .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.Code).IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();


        builder.Property(t => t.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

    }
}
